/**
 * Boot file for initializing MSAL for SSO with Azure AD B2C
 * See: https://forum.quasar-framework.org/topic/7992/incorporating-vue-msal-browser
 * See: https://www.npmjs.com/package/vue.msal2.connect
 */

import { AccountInfo, AuthenticationResult, BrowserCacheLocation, Configuration, EndSessionRequest, InteractionRequiredAuthError, LogLevel, PopupRequest, PublicClientApplication, RedirectRequest, SilentRequest, SsoSilentRequest } from '@azure/msal-browser'
import { ThrottlingUtils } from '@azure/msal-common'
import { AccountWithToken } from '../models/viewModels'
import { useAppStore } from '../stores/appStore'

/**
 * Wrapper class for the MSAL functionality based on:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/samples/msal-browser-samples/TypescriptTestApp2.0/src/AuthModule.ts
 */
export class MsalWrapper {
    private msalClient: PublicClientApplication // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-browser/classes/_src_app_publicclientapplication_.publicclientapplication.html
    private account: AccountInfo | null // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-common/modules/_src_account_accountinfo_.html
    private loginRedirectRequest: RedirectRequest // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-browser/modules/_src_request_redirectrequest_.html
    private loginRequest: PopupRequest // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-browser/modules/_src_request_popuprequest_.html
    private profileRedirectRequest: RedirectRequest
    private profileRequest: PopupRequest
    private mailRedirectRequest: RedirectRequest
    private mailRequest: PopupRequest
    private silentProfileRequest: SilentRequest // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-browser/modules/_src_request_silentrequest_.html
    private silentMailRequest: SilentRequest
    private silentLoginRequest: SsoSilentRequest
    private pendingAuthResponse: Promise<AuthenticationResult | null>
    private initiating: boolean

    constructor(config: Configuration) {
        this.msalClient = new PublicClientApplication(config)
        this.account = null
        this.initiating = false

        this.loginRequest = {
            scopes: [],
        }

        this.loginRedirectRequest = {
            ...this.loginRequest,
            redirectStartPage: window.location.href
        }

        this.profileRequest = {
            scopes: []
        }

        this.profileRedirectRequest = {
            ...this.profileRequest,
            redirectStartPage: window.location.href
        }

        // Add here scopes for access token to be used at MS Graph API endpoints.
        this.mailRequest = {
            scopes: ['Mail.Read']
        }

        this.mailRedirectRequest = {
            ...this.mailRequest,
            redirectStartPage: window.location.href
        }

        this.silentProfileRequest = {
            scopes: ['openid', 'profile',],
            forceRefresh: false
        }

        this.silentMailRequest = {
            scopes: ['openid', 'profile', 'mail.read'],
            forceRefresh: false
        }

        this.silentLoginRequest = {
            loginHint: 'IDLAB@msidlab0.ccsctp.net'
        }

        this.msalClient.handleRedirectPromise()
            .then(async (tokenResponse) => {
                await this.handleResponse(tokenResponse)
            })
            .catch((error) => {
                console.log(`Error during login redirect: ${error}`)
            })

        this.pendingAuthResponse = this.msalClient.handleRedirectPromise()
    }

    public async initiate() {
        const response = await this.pendingAuthResponse
        await this.handleResponse(response)
    }

    /**
     * Gets the current account if logged in and present.
     */
    public get currentAccount(): AccountInfo | null {
        return this.account
    }

    /**
     * Calls getAllAccounts and determines the correct account to sign into, currently defaults to first account found in cache.
     * TODO: Add account chooser code
     *
     * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-common/docs/Accounts.md
     */
    private getAccount(): AccountInfo | null {
        // need to call getAccount here?
        const currentAccounts = this.msalClient.getAllAccounts()
        if (currentAccounts === null) {
            console.log('No accounts detected')
            return null
        }

        if (currentAccounts.length > 1) {
            // Add choose account code here
            console.log('Multiple accounts detected, need to add choose account code.')
            return currentAccounts[0]
        } else if (currentAccounts.length === 1) {
            return currentAccounts[0]
        }

        return null
    }

    /**
     * Checks whether we are in the middle of a redirect and handles state accordingly. Only required for redirect flows.
     *
     * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/initialization.md#redirect-apis
     */
    loadAuthModule(): void {
        console.log('loadAuthModule')
        this.msalClient.handleRedirectPromise().then(async (resp: AuthenticationResult | null) => {
            await this.handleResponse(resp)
        }).catch(console.error)
    }

    /**
     * Handles the response from a popup or redirect. If response is null, will check if we have any accounts and attempt to sign in.
     * @param response
     */
    async handleResponse(response: AuthenticationResult | null) {
        let idToken

        if (response !== null) {
            this.account = response.account
            idToken = response.idToken
        } else {
            this.account = this.getAccount()
            // [CC] This step gets the tokens silently since we don't have the response if user refreshes page
            this.msalClient.setActiveAccount(this.account)
            const result = await this.msalClient.acquireTokenSilent(this.silentProfileRequest)
            //console.log(result)
            idToken = result.idToken
        }

        const appStore = useAppStore();

        if (this.account && !appStore.identity && !this.initiating) {
            this.initiating = true;

            // UIManager.showWelcomeMessage(this.account)

            await appStore.setIdentity(
                {
                    account: this.account,
                    idToken: idToken
                });
        }
    }

    /**
     * Calls ssoSilent to attempt silent flow. If it fails due to interaction required error, it will prompt the user to login using popup.
     * @param request
     */
    attemptSsoSilent() {
        console.log('attemptSsoSilent')
        this.msalClient.ssoSilent(this.silentLoginRequest).then(async () => {
            this.account = this.getAccount()
            if (this.account) {
                //UIManager.showWelcomeMessage(this.account)
                console.log('Performing silent SSO...')

                const appStore = useAppStore();

                if (this.account && !appStore.identity&& !this.initiating) {

                    this.initiating = true;

                    await appStore.setIdentity(
                        {
                            account: this.account,
                            idToken: '' // TODO: ???
                        });
                }
            } else {
                console.log('No account!')
            }
        }).catch(error => {
            console.error(`Silent Error: ${error}`)
            if (error instanceof InteractionRequiredAuthError) {
                this.login('loginPopup')
            }
        })
    }

    /**
     * Calls loginPopup or loginRedirect based on given signInType.
     * @param signInType
     */
    login(signInType: string): void {
        console.log('login')
        if (signInType === 'loginPopup') {
            this.msalClient.loginPopup(this.loginRequest).then(async (resp: AuthenticationResult) => {
                await this.handleResponse(resp)
            }).catch(console.error)
        } else if (signInType === 'loginRedirect') {
            void this.msalClient.loginRedirect(this.loginRedirectRequest)
        }
    }

    /**
     * Convenience method to avoid the string parameter.
     */
    loginPopup(): void {
        this.login('loginPopup')
    }

    /**
     * Convenience method to avoid the string parameter.
     */
    loginRedirect(): void {
        this.login('loginRedirect')
    }

    /**
     * Logs out of current account.
     */
    logout(): void {
        let account: AccountInfo | undefined
        if (this.account) {
            account = this.account
        }
        const logOutRequest: EndSessionRequest = {
            account
        }

        void this.msalClient.logoutRedirect(logOutRequest)
    }

    /**
     * Gets the token to read user profile data from MS Graph silently, or falls back to interactive redirect.
     */
    async getProfileTokenRedirect(): Promise<string | null> {
        console.log('getProfileTokenRedirect')
        if (this.account) {
            this.silentProfileRequest.account = this.account
        }
        return this.getTokenRedirect(this.silentProfileRequest, this.profileRedirectRequest)
    }

    /**
     * Gets the token to read user profile data from MS Graph silently, or falls back to interactive popup.
     */
    async getProfileTokenPopup(): Promise<string | null> {
        console.log('getProfileTokenPopup')
        if (this.account) {
            this.silentProfileRequest.account = this.account
        }
        return this.getTokenPopup(this.silentProfileRequest, this.profileRequest)
    }

    /**
     * Gets the token to read mail data from MS Graph silently, or falls back to interactive redirect.
     */
    async getMailTokenRedirect(): Promise<string | null> {
        console.log('getMailTokenRedirect')
        if (this.account) {
            this.silentMailRequest.account = this.account
        }
        return this.getTokenRedirect(this.silentMailRequest, this.mailRedirectRequest)
    }

    /**
     * Gets the token to read mail data from MS Graph silently, or falls back to interactive popup.
     */
    async getMailTokenPopup(): Promise<string | null> {
        console.log('getMailTokenPopup')
        if (this.account) {
            this.silentMailRequest.account = this.account
        }
        return this.getTokenPopup(this.silentMailRequest, this.mailRequest)
    }

    /**
     * Gets a token silently, or falls back to interactive popup.
     */
    private async getTokenPopup(silentRequest: SilentRequest, interactiveRequest: PopupRequest): Promise<string | null> {
        console.log('getTokenPopup')
        try {
            const response: AuthenticationResult = await this.msalClient.acquireTokenSilent(silentRequest)
            return response.accessToken
        } catch (e) {
            console.log('silent token acquisition fails.')
            if (e instanceof InteractionRequiredAuthError) {
                console.log('acquiring token using redirect')
                return this.msalClient.acquireTokenPopup(interactiveRequest).then((resp) => {
                    return resp.accessToken
                }).catch((err) => {
                    console.error(err)
                    return null
                })
            } else {
                console.error(e)
            }
        }

        return null
    }

    /**
     * Gets a token silently, or falls back to interactive redirect.
     */
    private async getTokenRedirect(silentRequest: SilentRequest, interactiveRequest: RedirectRequest): Promise<string | null> {
        console.log('getTokenRedirect')
        try {
            const response = await this.msalClient.acquireTokenSilent(silentRequest)
            return response.accessToken
        } catch (e) {
            console.log('silent token acquisition fails.')
            if (e instanceof InteractionRequiredAuthError) {
                console.log('acquiring token using redirect')
                this.msalClient.acquireTokenRedirect(interactiveRequest).catch(console.error)
            } else {
                console.error(e)
            }
        }

        return null
    }
}

const msalConfig = {
    auth: {
        clientId: import.meta.env.VITE_AZURE_AD_B2C_CLIENT_ID || '',
        authority: 'https://zytonomy.b2clogin.com/zytonomy.onmicrosoft.com/B2C_1_registration', // TODO: Parameterize this.
        redirectUri: import.meta.env.VITE_APP_BASE_URL,
        knownAuthorities: ['zytonomy.b2clogin.com'] // TODO: Parameterize this.
    },
    cache: {
        cacheLocation: BrowserCacheLocation.SessionStorage,
        storeAuthStateInCookie: false // Set this to 'true' if you are having issues on IE11 or Edge
    },
    system: {
        loggerOptions: {
            loggerCallback: (level: any, message: any, containsPii: any) => {
                if (containsPii) {
                    return
                }
                switch (level) {
                    case LogLevel.Error:
                        console.error(message)
                        return
                    case LogLevel.Info:
                        console.info(message)
                        return
                    case LogLevel.Verbose:
                        console.debug(message)
                        return
                    case LogLevel.Warning:
                        console.warn(message)
                }
            }
        }
    }
}

const auth = new MsalWrapper(msalConfig);

export { auth }