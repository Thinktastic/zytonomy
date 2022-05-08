/// <reference types="vite/client" />

declare module '*.vue' {
    import { DefineComponent } from 'vue'
    // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/ban-types
    const component: DefineComponent<{}, {}, any>
    export default component
}

interface ImportMetaEnv {
    VITE_AZURE_AD_B2C_CLIENT_ID: string;
    VITE_API_ENDPOINT: string;
    VITE_APP_BASE_URL: string;
}
