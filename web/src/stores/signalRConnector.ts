import * as SignalR from '@microsoft/signalr'
import { deserialize } from 'class-transformer'
import { GenericRef, Message, Note, Comment, Invitation, Workspace } from '../models/viewModels'
import { Notify } from 'quasar'
import { useStore } from '.'
import { PiniaPluginContext } from 'pinia'

class SignalRService {
    public async connect(idToken: string) {
        const $store = useStore();

        const connection = new SignalR.HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_ENDPOINT}/api`, {
                accessTokenFactory: () => idToken
            })
            .configureLogging(SignalR.LogLevel.Information)
            .withAutomaticReconnect()
            .build()

        // Register handlers here.
        connection.on('user-mutate-add-workspace', (workspaceRef: string) => {
            console.log("  RECEIVED RESPONSE user-mutate-add-workspace");
            void $store.data.addWorkspace(deserialize(GenericRef, workspaceRef));
        })

        connection.on('user-accept-invitation', (workspaceRef: string) => {
            console.log("  RECEIVED RESPONSE user-accept-invitation");
            void $store.data.acceptedInvitation(deserialize(GenericRef, workspaceRef));
        })

        connection.on('user-invited-to-workspace', (invitation: string) => {
            console.log("  RECEIVED RESPONSE user-invited-to-workspace");
            void $store.data.receivedInvitation(deserialize(Invitation, invitation));
        })

        connection.on('workspace-documents-provisioned', (workspace: string) => {
            // This is raised when the workspace is created for the first time and
            // the documents are already provisioned.
            console.log("  RECEIVED RESPONSE workspace-documents-provisioned");
            void $store.data.workspaceDocumentsProvisioned();
        })

        connection.on('workspace-documents-added', (workspace: string) => {
            // This is raised when the workspace has already been created and a nwe document
            // has been successfully added to the workspace and published.
            console.log("  RECEIVED RESPONSE workspace-documents-added");
            void $store.data.workspaceSourcesAdded(deserialize(Workspace, workspace))

            Notify.create({
                message: 'A new document was added to the workspace',
                icon: 'mdi-file-check-outline',
                color: 'dark',
                textColor: 'white'
            })
        })

        connection.on('workspace-source-deleted', (workspace: string) => {
            // This is raised when the workspace has been updated after a document
            // has been successfully removed from the workspace.
            console.log("  RECEIVED RESPONSE workspace-source-deleted");
            void $store.data.workspaceSourceRemoved(deserialize(Workspace, workspace))

            Notify.create({
                message: 'A file was removed from the workspace',
                icon: 'mdi-file-cancel-outline',
                color: 'dark',
                textColor: 'white'
            })
        })

        connection.on('workspace-deleted-for-user', (workspaceId: string) => {
            // This is raised when the workspace has been updated after a document
            // has been successfully removed from the workspace.
            console.log("  RECEIVED RESPONSE workspace-deleted-for-user");
            void $store.data.workspaceDeleted(workspaceId)

            Notify.create({
                message: 'A workspace has been deleted.',
                icon: 'mdi-cube-off-outline',
                color: 'dark',
                textColor: 'white'
            })
        })

        connection.on('workspace-chat-received', (messageJson: string) => {
            console.log("  RECEIVED RESPONSE workspace-chat-received");
            void $store.messaging.receivedChat(deserialize(Message, messageJson))
        })

        connection.on('workspace-question-received', (messageJson: string) => {
            console.log("  RECEIVED RESPONSE workspace-question-received");
            void $store.messaging.receivedQuestion(deserialize(Message, messageJson))
        })

        connection.on('qna-question-answered', (messageJson: string) => {
            console.log("  RECEIVED RESPONSE qna-question-answered");
            void $store.messaging.receivedAnswer(deserialize(Message, messageJson))
        })

        connection.on('workspace-note-saved', (noteJson: string) => {
            console.log("  RECEIVED RESPONSE workspace-note-saved");
            void $store.data.receivedNote(deserialize(Note, noteJson))
        })

        connection.on('workspace-note-deleted', (noteJson: string) => {
            console.log("  RECEIVED RESPONSE workspace-note-deleted");
            void $store.data.deletedNote(deserialize(Note, noteJson));
        })

        connection.on('workspace-note-comment-added', (commentJson: string) => {
            console.log("  RECEIVED RESPONSE workspace-note-comment-added");
            void $store.data.receivedComment(deserialize(Comment, commentJson))
        })

        await connection.start()
    }
}

export function signalRConnector(context: PiniaPluginContext) {
    // Set the store so we have access to trigger events.
    const signalRService = new SignalRService();

    // When the appStore sets the user info, it will return a token.
    // We pass the token to the connector to initialize the connection
    // with SignalR.
    context.store.$onAction(async ({ name, store, after}) => {
        if(store.$id !== 'appStore' || name !== 'getUserInfo') {
            return;
        }

        after((result) => {
            signalRService.connect(result);
        })
    })
}