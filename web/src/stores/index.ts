import { useAppStore } from "./appStore";
import { useDataStore } from "./dataStore";
import { useMessagingStore } from "./messagingStore";

export function useStore() {
    const app = useAppStore();
    const data = useDataStore();
    const messaging = useMessagingStore();

    return {
        app,
        data,
        messaging
    }
}