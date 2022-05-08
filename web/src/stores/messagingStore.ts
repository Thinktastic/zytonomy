import { defineStore } from 'pinia';
import { GenericRef, Message, MessageType, WorkspaceMessageList } from '../models/viewModels';
import { api } from '../boot/axios';

export interface MessagingStateInterface {
    activeMessageList: WorkspaceMessageList | null
    messageLists: WorkspaceMessageList[]
}

export const useMessagingStore = defineStore('messagingStore', {
    state: (): MessagingStateInterface => {
        return {
            activeMessageList: null,
            messageLists: []
        }
    },
    actions: {
        /**
         * Add a new message list.
         */
        addAndSetActiveMessageList(workspaceId: string) {
            const messageList = {
                workspaceId: workspaceId,
                messages: []
            };

            this.messageLists.push(messageList);

            this.setActiveMessageList(messageList);
        },

        setActiveMessageList(messageList: WorkspaceMessageList) {
            this.activeMessageList = messageList;
        },

        /**
         * Sends a message to the API endpoint.
         */
        async sendMessage(message: Message) {
            // Add the message locally first
            const list = this.messageLists.find(l => l.workspaceId === message.workspaceId);

            if(list) {
                list.messages.push(message);
            }

            await api.post('messaging/chat', message);
        },

        /**
         * Sends a question to the API endpoint.
         */
        async sendQuestion(message: Message) {
            // Add the message locally first
            const list = this.messageLists.find(l => l.workspaceId === message.workspaceId);

            if(list) {
                list.messages.push(message);
            }

            await api.post('messaging/ask', message);
        },

        /**
         * Joins the user to the workspace
         */
        async joinWorkspace(workspaceId: string) {
            await api.get(`realtime/join/${workspaceId}`);
        },

        /**
         * The SignalRPlugin has received a CHAT MESSAGE for a workspace that this user is connected to.
         */
        receivedChat(message: Message) {
            const list = this.messageLists.find(l => l.workspaceId === message.workspaceId);

            if(list) {
                const index = list.messages.findIndex(m => m.id == message.id);

                index < 0 ? list.messages.push(message) : list.messages.splice(index, 1, message);
            }
        },

        /**
         * The SignalRPlugin has received a QUESTION for a workspace that this user is connected to.
         * In the case of a question, we want to replace the original "outgoing" message and then
         * also inject another message representing the placeholder that indicates that the system is
         * retrieving the results.
         */
        receivedQuestion(message: Message) {
            console.log('RECEIVED QUESTION!');
            console.log(message);

            // Replace the out-going pending message
            const list = this.messageLists.find(l => l.workspaceId === message.workspaceId);

            if(list) {
                const index = list.messages.findIndex(m => m.id == message.id);

                index < 0 ? list.messages.push(message) : list.messages.splice(index, 1, message);
            }

            // Add the placeholder message
            const bot = new GenericRef();
            bot.id = '';
            bot.name = 'Thinktastic';

            const placeholder = new Message();
            placeholder.id = `placeholder_response_${message.id}`; // See QnAEndpoints.cs
            placeholder.workspaceId = message.workspaceId;
            placeholder.parentMessageId = message.id;
            placeholder.posted = false;
            placeholder.author = bot;
            placeholder.body = 'I\'m looking up the answer for you.';
            placeholder.messageType = MessageType.BOT_PLACEHOLDER;

            list?.messages.push(placeholder);
        },

        /**
         * The answer contains JSON in the body which has the details of the response.
         */
        receivedAnswer(message: Message) {
            const list = this.messageLists.find(l => l.workspaceId === message.workspaceId);

            console.log('RECEIVED ANSWER!');

            if(list) {
                const index = list.messages.findIndex(m => m.id == message.id);

                index < 0 ? list.messages.push(message) : list.messages.splice(index, 1, message);
            }
        }
    }
});