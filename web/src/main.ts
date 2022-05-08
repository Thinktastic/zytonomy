// FILE: main.js

import { createApp } from 'vue';
import { createPinia } from 'pinia';
import axios from 'axios';
import { Quasar } from 'quasar';
import quasarIconSet from 'quasar/icon-set/mdi-v6';
import { AppFullscreen, LoadingBar, Notify, Dialog } from 'quasar';
import utc from 'dayjs/plugin/utc';
import relativeTime from 'dayjs/plugin/relativeTime';

// Import icon libraries; you can choose different ones!
// See: https://quasar.dev/start/vite-plugin#using-quasar
import '@quasar/extras/roboto-font-latin-ext/roboto-font-latin-ext.css'
import '@quasar/extras/mdi-v6/mdi-v6.css'

// A few examples for animations from Animate.css:
import '@quasar/extras/animate/fadeIn.css'
import '@quasar/extras/animate/fadeOut.css'
import '@quasar/extras/animate/flipInX.css'
import '@quasar/extras/animate/fadeInDown.css'
import '@quasar/extras/animate/fadeInUp.css'
import '@quasar/extras/animate/fadeOutUp.css'
import '@quasar/extras/animate/fadeOutDown.css'
import '@quasar/extras/animate/fadeInRight.css'
import '@quasar/extras/animate/fadeInLeft.css'
import '@quasar/extras/animate/fadeOutLeft.css'
import '@quasar/extras/animate/fadeOutRight.css'
import '@quasar/extras/animate/zoomIn.css'
import '@quasar/extras/animate/zoomOut.css'

// Import Quasar css
import 'quasar/src/css/index.sass'

// Assumes your root component is App.vue
// and placed in same folder as main.js
import App from './App.vue'
import router from './router'
import { api } from './boot/axios';
import { auth } from './boot/msal'
import dayjs from 'dayjs';
import { useStore } from './stores';
import { signalRConnector } from './stores/signalRConnector';

const myApp = createApp(App)

myApp.use(Quasar, {
    plugins: {
        Dialog: Dialog,
        Notify: Notify,
        LoadingBar: LoadingBar,
        AppFullscreen: AppFullscreen
    }, // import Quasar plugins and add here
    iconSet: quasarIconSet,
    config: {
        notify: {
            position: 'top',
            icon: 'mdi-message-video',
            timeout: 7500,
        },

        loadingBar: {
            color: 'primary',
            size: '8px',
            position: 'bottom'
        }
    }
})

// Connect Pinia
const pinia = createPinia();
pinia.use(signalRConnector);
myApp.use(pinia);
const store = useStore(); // Cause the store to initialize.

// Connect Vue Router
myApp.use(router);

// Assumes you have a <div id="app"></div> in your index.html
myApp.mount('#app')

// Set up global properties for Axios and the API
myApp.config.globalProperties.$axios = axios;
myApp.config.globalProperties.$api = api;

// Set up global properties for MSAL auth.
myApp.config.globalProperties.$msal = auth;
// https://forum.vuejs.org/t/how-to-use-globalproperties-in-vue-3-setup-method/108387/5
myApp.provide('msal', myApp.config.globalProperties.$msal)

// Set up dayjs
dayjs.extend(utc)
dayjs.extend(relativeTime)