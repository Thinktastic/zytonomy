import { AccountInfo } from '@azure/msal-browser'
import md5 from 'md5'

/**
 * This type is used from msal.ts.  The AccountInfo does not include the base64 idToken
 * which we also want to pass so we encapsulate it in another type.
 */
export type AccountWithToken = {
    account: AccountInfo
    idToken: string
}

/**
 * Interface class to support the AuthInfo.idTokenClaims object.
 */
interface ClaimTokens {
    given_name: string
    family_name: string
    emails: string[]
    newUser: boolean | undefined
}

/**
 * Generic reference type used to reference another object.  This type is used for modeling
 * embedded entity references from the CosmosDB documents.
 */
export class GenericRef {
    public id = ''
    public name = ''
    public containerName = ''
    public partitionKey = ''
}

/**
 * View model class which wraps the MSAL AccountInfo class and provides convenience accessors.
 * The identity entity is the raw identity information of the user provided from Azure AD whereas
 * the user entity is the local profile of the user stored in the CosmosDB backend.
 */
export class Identity {
    public id: string
    public firstName: string
    public lastName: string
    public email: string
    public isNewUser: boolean

    /**
     * Creates an instance using the MSAL AccountInfo set in msal.ts during login.
     * @param accountInfo
     */
    public constructor(accountInfo?: AccountInfo) {
        // Default user info if constructor is invoked with null (unauthenticated.)
        if (accountInfo == null) {
            this.id = ''
            this.firstName = 'Unauthenticated'
            this.lastName = 'User'
            this.email = ''
            this.isNewUser = false

            return
        }

        const tokens = accountInfo.idTokenClaims as ClaimTokens

        this.id = accountInfo.localAccountId
        this.firstName = tokens.given_name
        this.lastName = tokens.family_name
        this.email = tokens.emails[0]
        this.isNewUser = tokens.newUser || false
    }

    /**
     * The formatted display name for the user.
     */
    public get displayName(): string {
        return `${this.firstName} ${this.lastName}`
    }

    /**
     * The monogram for this user based on the first initials of the first and last names.
     */
    public get monogram () {
        return `${this.firstName[0]}${this.lastName[0]}`
    }

    /**
     * Gets a generic ref for this user.
     */
    public get asRef(): GenericRef {
        const r = new GenericRef()
        r.id = this.id
        r.name = `${this.firstName} ${this.lastName}`

        return r
    }
}

/**
 * Type that is used to define the workspace instantiation parameters.
 */
export type WorkspaceDefinition = {
    title: string
    description: string
    files: File[]
}

/**
 * Front-end view model for content sources.
 */
export class ContentSource {
    public displayName: string | null
    public originalFileName: string | null
    public blobStorageFileName: string | null
    public addedUtc: string | null
    public addedById: string | null
    public status = ''

    constructor () {
        this.displayName = null
        this.originalFileName = null
        this.blobStorageFileName = null
        this.addedUtc = null
        this.addedById = null
    }
}

/**
 * Represents a user-workspace mapping as a member with an attached role.
 */
export class Member {
    public roles: string[] = [];
    public user = new GenericRef();
    public addedUtc = '';
}

/**
 * Front-end view model for a workspace.
 */
export class Workspace {
    public id: string | null;
    public name: string | null;
    public description: string | null;
    public sources: ContentSource[] | null;
    public status: string | null;
    public createdBy: GenericRef | null;
    public alertCount = 0;
    public members: Array<Member> | null;

    constructor() {
        this.id = null;
        this.name = null;
        this.description = null;
        this.sources = null;
        this.status = null;
        this.createdBy = null;
        this.members = null;
    }
}

export enum MessageType {
    USER_CHAT = 'UserChat',
    USER_QUESTION = 'UserQuestion',
    USER_QUESTION_DIRECT = 'UserQuestionDirect',
    BOT_PLACEHOLDER = 'BotPlaceholder',
    BOT_RESPONSE = 'BotResponse',
    SAVED_NOTE = 'SavedNote',
    USER_CREATED_NOTE = 'UserCreatedNote',
    NOTE_COMMENT = 'NoteComment',
    MEETING_REQUEST = 'MeetingRequest',
    END_MEETING = 'EndMeeting'
}

/**
 * Chat message model.
 */
export class Message {
    public id = '';
    public workspaceId = '';
    public createdUtc = '';
    public parentMessageId?: string;
    public title?: string;
    public body = '';
    public author: GenericRef = new GenericRef();
    public posted = false;
    public messageType = MessageType.USER_CHAT;
    public targetId? = '';
}

/**
 * Used in state management for keeping the list of messages.
 */
export type WorkspaceMessageList = {
    workspaceId: string,
    messages: Message[]
};

/**
 * Comment class
 */
export class Comment {
    public author: GenericRef | null = null
    public body = ''
    public parentId = ''
    public createdUtc = ''
}

/**
 * Simple wrapper type for a comment to be added to a note.
 */
export type NewComment = {
    comment: Comment,
    noteId: string
}

/**
 * Model of the note which is persisted on the server.
 */
export class Note {
    public id: string
    public workspaceId: string
    public author: GenericRef

    public body = ''
    public name = ''
    public createdUtc = ''
    public icon = ''
    public color = ''
    public kbId = ''
    public kbEntryId = 0
    public source: ContentSource | null = null
    public tags = new Array<string>()
    public importance = ''
    public isPrivate = false
    public comments = new Array<Comment>()
    public firstVideoUrl: string | null = null
    public firstImageUrl: string | null = null

    constructor(author: Identity | null, workspaceId: string | null | undefined) {
        this.id = md5(`${Date.now().toString()}${author?.id}`)
        this.workspaceId = workspaceId || ''
        this.author = author?.asRef || new GenericRef()
    }
}

/**
 * Front-end model for filters for the messages used for search.
 */
 export type Filter = {
    /**
     * The value of the filter.
     */
    value: string

    /**
     * The label of the filter for displaying.
     */
    label: string

    /**
     * The type of the filter based on the facet
     */
    type: string

    /**
     * An optional field which is used for user filters.
     */
    avatar?: string
}

/**
 * Models an invitation which is sent to the server to invite a new user
 */
export class Invitation {
    email: string
    firstName: string
    lastName: string
    message: string
    invitedBy: GenericRef | null = null
    createdUtc = ''
    status = ''
    workspaceId = ''
    id = ''
    name = ''

    constructor(email: string, firstName: string, lastName: string, message: string) {
        this.email = email
        this.firstName = firstName
        this.lastName = lastName
        this.message = message
    }
}