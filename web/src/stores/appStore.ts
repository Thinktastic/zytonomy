import { defineStore, } from 'pinia';
import { plainToInstance } from 'class-transformer'
import { AccountWithToken, GenericRef, Identity, Invitation } from '../models/viewModels';
import { api } from '../boot/axios';
import { useDataStore } from './dataStore';

export type TabType = 'notes_tab' | 'learning_tab' | 'meetings_tab';

export interface AppStateInterface {
    identity: Identity | null
    idToken: string
    startingUp: boolean
    rightDrawerOpen: boolean
    workspaceTab: TabType
}

export const useAppStore = defineStore('appStore', {
    state: (): AppStateInterface => {
        return {
            identity: null,
            idToken: '',
            startingUp: true,
            rightDrawerOpen: false,
            workspaceTab: 'notes_tab'
        }
    },
    actions: {
        /**
         * Sets the current identity based on the MSAL account information.  The token
         * we've received from Azure AD B2C cannot be used directly by functions; we are
         * going to exchange the token with a custom generated JWT token and then use the
         * custom JWT token instead.  The process of exchange will verify the token.
         * @param accountWithToken The MSAL AccountInfo to set for the current user.
         */
        async setIdentity(accountWithToken: AccountWithToken) {
            // Send a request using the Azure AD B2C provided token.
            const result = await api.post('identity/verify', accountWithToken.idToken, {
                headers: {
                    'Authorization': accountWithToken.idToken
                }
            });

            // Set the axios API handle to use the token that we get back in exchange.
            api.interceptors.request.use(
                (config) => {
                    if(config.headers) {
                        config.headers['Authorization'] = result.data;
                    }

                    return config;
                }
            );

            this.idToken = result.data;
            this.identity = new Identity(accountWithToken.account);
            await this.getUserInfo();
        },

        /**
         * Retrieves the user from the API endpoint.  If the return is null, the user has not
         * been created in the database yet so we should make the call to create the user.
         */
        async getUserInfo() {
            const identity = this.identity;

            if (identity != null) {
                // Get the user from the API which will have the user's content sets.
                const getUserResult = await api.get(`user/${identity.id}`)

                let workspaces: GenericRef[];

                if(getUserResult.status === 204) {
                    // Response was empty; user does not exist; create the user.
                    await api.post('user/create', identity)
                    // TODO: Handle 500 status here

                    // This is a new user so the user will have no content sets.
                    workspaces = []
                }
                else {
                    // User exists; grab the content-sets associated with the user.
                    workspaces = plainToInstance(GenericRef, getUserResult.data.workspaces as [])
                }

                const dataStore = useDataStore();

                dataStore.setWorkspaces(workspaces);

                this.startingUp = false;
            }

            return this.idToken; // This allows the subscription to receive this value.
        },

        /**
         * Initiates an invitation to the user.
         */
        async inviteUser(invite: Invitation) {
            const dataStore = useDataStore();

            await api.post(`workspace/${dataStore.activeWorkspace?.id}/invite`, invite)
        },

        /**
         * Sets the tab in the workspace to allow loading a specific tab without routing.
         */
        setWorkspaceTab(tab: TabType) {
            this.workspaceTab = tab;
        }
    }
});