import { defineStore } from 'pinia';
import { plainToInstance } from 'class-transformer';
import { Notify } from 'quasar';
import { api } from '../boot/axios';
import { KbEntry } from '../models/kbModels';
import { Comment, ContentSource, Filter, GenericRef, Invitation, NewComment, Note, Workspace, WorkspaceDefinition } from '../models/viewModels';
import { useMessagingStore } from './messagingStore';

export interface DataStateInterface {
    workspaces: GenericRef[]
    loadedWorkspaces: Workspace[]
    activeWorkspace: Workspace | null
    provisioningWorkspaceId: string | null
    pendingEntry: KbEntry | null
    activeWorkspaceNotes: Note[]
    activeWorkspaceInvitations: Invitation[]
    userPendingInvitations: Invitation[]
    filters: Filter[],
    securePdfUrl: string | null
    targetWorkspaceId: string | null
    removedWorkspaceId: string | null
}

export const useDataStore = defineStore('dataStore', {
    state: (): DataStateInterface => {
        return {
            workspaces: [],
            loadedWorkspaces: [],
            activeWorkspace: null,
            provisioningWorkspaceId: null,
            pendingEntry: null,
            activeWorkspaceNotes: [],
            activeWorkspaceInvitations: [],
            userPendingInvitations: [],
            filters: [],
            securePdfUrl: null,
            targetWorkspaceId: null,
            removedWorkspaceId: null
        }
    },
    actions: {
        /**
         * Sets the content sets for the user after it has been retrieved from the app
         * module actions.
         */
        setWorkspaces(workspaces: GenericRef[]) {
            this.workspaces = workspaces;
        },

        /**
         * Loads a workspace from the backend service if it hasn't already been loaded
         * previously.  Otherwise, set the active workspace from a loaded workspace.
         */
        async loadWorkspace(workspaceId: string) {
            console.log(`  WORKSPACE ID: ${workspaceId}`);

            let loadedWorkspace = this.loadedWorkspaces.find(w => w.id === workspaceId)

            if(!loadedWorkspace) {
                const workspaceResult = await api.get(`workspace/${workspaceId}`);
                loadedWorkspace = workspaceResult.data as Workspace;
                this.loadedWorkspaces.push(loadedWorkspace);

                const messagingStore = useMessagingStore();

                // Join the workspace chat.
                await messagingStore.joinWorkspace(workspaceId);
            }

            console.log(`  SETTING THE ACTIVE WORKSPACE...`);

            // When we load the workspace, we also want to set the alert count to 0
            this.activeWorkspace = loadedWorkspace;
            this.activeWorkspace.alertCount = 0;
        },

        /**
         * Loads the notes for a given workspace from the back end.
         */
        async loadWorkspaceNotes(workspaceId: string) {
            if(this.activeWorkspaceNotes.length > 0 &&
                this.activeWorkspaceNotes[0].workspaceId === workspaceId) {
                    // Already loaded the state for this workspace.
                    return;
                }

            const notesResult = await api.get(`notes/workspace/${workspaceId}`);

            this.activeWorkspaceNotes = plainToInstance(Note, notesResult.data as []);
        },

        /**
         * Creates a workspace using the workspace definition.
         */
        async createWorkspace(workspace: WorkspaceDefinition) {
            const form = new FormData();

            // Append files.
            workspace.files.forEach(file => {
                form.set(file.name, file);
            });

            // Append keys.
            form.set('title', workspace.title);
            form.set('description', workspace.description);

            const createWorkspaceResult = await api.post('workspace/create', form, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            });

            this.provisioningWorkspaceId = createWorkspaceResult.data;
        },

        /**
         * Adds a document source to the workspace which is sent to the server to be included in
         * the knowledge base.  This will require a build and publish of the workspace.
         */
        async addDocumentSource(files: File[]) {
            const form = new FormData()

            // Append files.
            files.forEach(file => {
                form.set(file.name, file);

                const source = new ContentSource();
                source.status = 'Publishing';
                source.displayName = file.name;
                source.originalFileName = file.name;
                source.blobStorageFileName = file.name;
                source.addedUtc = new Date().toUTCString();

                // Add the document locally to show progress; however the file may take several minutes to replace.
                this.activeWorkspace?.sources?.push(source);
            });

            const workspaceId = this.activeWorkspace?.id

            await api.post(`workspace/${workspaceId}/files/add`, form, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            });

            // Wait for feedback from SignalR
        },

        /**
         * Removes a source from the workspace at the given index.  This will trigger an event
         * that will be broadcast to the clients via SignalR to remove the source.
         */
        async deleteSource(index: number) {
            const workspaceId = this.activeWorkspace?.id;

            // Make the API call to do the delete
            await api.delete(`workspace/${workspaceId}/sources/remove/${index}`);

            // Change the status of the instance held locally
            const source = this.activeWorkspace?.sources?.[index];

            if(!source) {
                return;
            }

            source.status = 'Deleting';

            this.activeWorkspace?.sources?.splice(index, 1, source);
        },

        /**
         * Adds a workspace for the user.  This is generally a response to either
         * a workspace being created by the user or the user being added to a workspace
         */
        addWorkspace(workspaceRef: GenericRef) {
            if(!this.workspaces) {
                this.workspaces = <GenericRef[]>[];
            }

            console.log('ADDING WORKSPACE')

            this.workspaces.push(workspaceRef)

            console.log('ADDED WORKSPACE')
        },

        /**
         * Initiate the deletion of the workspace. This process will trigger a notification via SignalR
         * for all connected users that will redirect them to the home page.
         */
        async deleteWorkspace(workspaceId: string) {
            try {
                await api.delete(`workspace/${workspaceId}`)
            }
            catch (error) {
                Notify.create({
                    message: 'You do not have permissions to delete this workspace',
                    icon: 'mdi-alert-circle-outline',
                    color: 'red-9',
                    textColor: 'white'
                })
            }
        },

        /**
         * SignalR handler when a workspace has been confirmed to be deleted either by a user.
         */
        workspaceDeleted(workspaceId: string) {
            // Set the value to trigger routing
            this.removedWorkspaceId = workspaceId

            // Remove from loaded workspaces
            let index = this.loadedWorkspaces.findIndex(w => w.id === workspaceId)

            if (index > -1) {
                this.loadedWorkspaces.splice(index, 1)
            }

            // Remove from user's workspaces
            index = this.workspaces.findIndex(w => w.id === workspaceId)

            if (index > -1) {
                this.workspaces.splice(index, 1)
            }

            // Remove invitations?
            index = this.userPendingInvitations.findIndex(w => w.workspaceId === workspaceId)

            if (index > -1) {
                this.userPendingInvitations.splice(index, 1)
            }

            // Set the active workspace to null.
            if (this.activeWorkspace?.id === workspaceId) {
                this.activeWorkspace == null
            }
        },

        /**
         * The user has accepted an invitation and the server has returned a response via
         * SignalR that the user has been added to the workspace and the workspace has
         * been added to the user.  Remove any invitations for this workspace and add the
         * workspace to the user.
         */
        acceptedInvitation( workspaceRef: GenericRef) {
            console.log(workspaceRef);

            if(!this.workspaces) {
                this.workspaces = new Array<GenericRef>();
            }

            console.log('ADDING WORKSPACE');

            this.workspaces.push(workspaceRef);

            console.log('ADDED WORKSPACE');

            const index = this.userPendingInvitations.findIndex(i => i.workspaceId === workspaceRef.id);

            this.userPendingInvitations.splice(index, 1);

            console.log(`SETTING TARGET WORKSPACE ID: ${workspaceRef.id}`);

            // Router is not available outside of setup so we need to make a state change that
            // we listen to elsewhere to make this navigation.
            this.targetWorkspaceId = workspaceRef.id;

            console.log('SET TARGET WORKSPACE ID');
        },

        /**
         * An invitation has been generated for an existing user and received via SignalR.
         */
        receivedInvitation(invitation: Invitation) {
            if(this.userPendingInvitations === null) {
                this.userPendingInvitations = <Invitation[]>[];
            }

            this.userPendingInvitations.unshift(invitation)
        },

        /**
         * Resets the provisioning ID after a workspace has been successfully provisioned.
         */
        workspaceDocumentsProvisioned() {
            this.provisioningWorkspaceId = null;
        },

        /**
         * When new documents are added successfully (pulished called on the KB), the
         * document status gets updated to "Published".  Update the front-end model
         * for the workspace.
         */
        workspaceSourcesAdded(workspace:Workspace) {
            if (this.activeWorkspace?.id === workspace.id) {
                this.activeWorkspace = workspace;
            }

            const index = this.loadedWorkspaces.findIndex(w => w.id === workspace.id);;

            if (index > -1) {
                this.loadedWorkspaces.splice(index, 1, workspace);
            }
        },

        /**
         * When a document has been removed from the workspace, this update from SignalR
         * indicates that the corresponding KB items have been removed based on the source
         * name.
         */
        workspaceSourceRemoved(workspace:Workspace) {
            if (this.activeWorkspace?.id === workspace.id) {
                this.activeWorkspace = workspace;
            }

            const index = this.loadedWorkspaces.findIndex(w => w.id === workspace.id);;

            if (index > -1) {
                this.loadedWorkspaces.splice(index, 1, workspace);
            }
        },
        /**
         * User is saving a KB entry as a note.  Triggered from AnswerMessage.vue.  This should
         * cause the save note dialog to become visible.
         */
        setPendingEntry(entry: KbEntry | null) {
            this.pendingEntry = entry;
        },

        /**
         * Initiates the action to save the note via the API.  This will generate a SignalR
         * response once the note is saved (SignalRPlugin.ts via workspace-note-saved)
         */
        async saveNote(note: Note) {
            await api.post('notes/save', note);
        },

        /**
         * Deletes a note from the back end.
         */
        async deleteNote(note: Note) {
            await api.delete(`notes/delete/${note.id}`);

            // Delete it locally
            this.deletedNote(note);
        },

        /**
         * A note has been published to the remote server and transmitted to the client via
         * SignalR.
         */
        receivedNote(note: Note) {
            // Check to see if the active workspace is the same as the one for this note and
            // if so, add it to the workspace.
            if(this.activeWorkspace &&
                this.activeWorkspace.id === note.workspaceId) {
                    this.activeWorkspaceNotes.unshift(note);

                    return;
                }

            // If not, change the workspace to flag it with a notification
            const index = this.loadedWorkspaces.findIndex(w => w.id === note.workspaceId)

            if(index === -1) {
                return;
            }

            // Splice it to trigger updates.
            const workspace = this.loadedWorkspaces[index]

            workspace.alertCount++

            this.loadedWorkspaces.splice(index, 1, workspace)
        },

        /**
         * A note has been deleted and a SignalR message has been received.  The note
         * that is received is a partial object with only the workspace ID and note ID.
         */
        deletedNote(note: Note) {
            if(!this.activeWorkspaceNotes) {
                return;
            }

            const index = this.activeWorkspaceNotes.findIndex(n => n.id === note.id);

            if(index < 0) {
                return;
            }

            this.activeWorkspaceNotes.splice(index, 1);
        },

        /**
         * Submits a comment to be added to a note.  The note is the parent ID of the comment.
         */
        async addComment(newComment: NewComment) {
            await api.post(`notes/${newComment.noteId}/comments/add`, newComment.comment)
        },

        /**
         * Add a comment that has been added to a note and transmitted to the client via
         * SignalR.  TODO: This mechanism only allows comments directly attached to the
         * note since the parentId is the only mechanism linking it to the note; consider
         * adding another field later.
         */
        receivedComment(comment: Comment) {
            // Only search notes for the current workspace.
            // TODO: Maybe need to keep a global map for comments against notes?
            if(!this.activeWorkspaceNotes) {
                return;
            }

            const index = this.activeWorkspaceNotes.findIndex(n => n.id === comment.parentId);

            if(index < 0) {
                return;
            }

            const note = this.activeWorkspaceNotes[index];

            note.comments.unshift(comment);
        },

        /**
         * Sets the filters for the selected site messages.
         */
        setFilters (filters: Filter[]) {
            this.filters = filters;
        },

        /**
         * The filter to add to the global state
         */
        addFilter (filter: Filter) {
            this.filters.push(filter);
        },

        /**
         * Clears the filters currently held.
         */
        clearFilters() {
            this.filters.splice(0, this.filters.length);
        },

        /**
         * Removes a filter from the state.
         */
        removeFilter(index: number) {
            this.filters.splice(index, 1);
        },

        /**
         * Initiates the server call to retrieve the secure access URL for the content at the index
         * for the active workspace
         */
        async getSecureDocumentUrl(index: number) {
            const workspaceId = this.activeWorkspace?.id;

            const result = await api.get(`workspace/${workspaceId}/content/${index}/secure`);

            this.securePdfUrl = result.data;
        },

        /**
         * Get the pending invitations for the active workspace.
         */
        async getPendingInvitations() {
            const workspaceId = this.activeWorkspace?.id

            const result = await api.get(`workspace/${workspaceId}/invitations/Pending`)

            this.activeWorkspaceInvitations = plainToInstance(Invitation, result.data as []);
        },

        /**
         * Get the pending invitations for the user.
         */
        async getUserInvitations() {
            const result = await api.get('user/invitations/pending')

            this.userPendingInvitations = plainToInstance(Invitation, result.data as []);
        },

        /**
         * User accepts a workspace invitation
         */
        async acceptWorkspaceInvitation(invitationId: string) {
            const index = this.userPendingInvitations.findIndex(i => i.id === invitationId)

            const invitation = this.userPendingInvitations[index]

            invitation.status = 'Accepting'

            this.userPendingInvitations.splice(index, 1, invitation)

            await api.get(`user/invitations/${invitationId}/accept`)
        }
    }
});
