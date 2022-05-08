import { RouteRecordRaw } from 'vue-router';
import { auth } from '../boot/msal'
import { useStore } from '../stores';

const routes: RouteRecordRaw[] = [
    {
        path: '/',
        component: () => import('../layouts/MainLayout.vue'),
        beforeEnter: async (to, from, next) => {
            console.log('CHECKING THE AUTH ROUTE');

            try {
                await auth.initiate()

                // This case occurs on the redirect back from the auth flow
                // which passes a token back to the RP.
                if (to.name === 'Error404') {
                    next('/');
                }

                next();
            }
            catch (error) {
                console.log(`Error during login redirect: ${error}`)

                if (to.path !== '/auth') {
                    next('/auth');
                }
            }
        },
        children: [
            {
                name: 'Home',
                path: '',
                component: () => import('../pages/Index.vue'),
                meta: { requiresAuth: true }
            },
            {
                name: 'WorkspaceCreate',
                path: 'workspace/create',
                component: () => import('../pages/WorkspaceCreate.vue'),
                meta: { requiresAuth: true }
            },
            {
                name: 'WorkspaceView',
                path: 'workspace/:id',
                component: () => import('../pages/WorkspaceView.vue'),
                meta: { requiresAuth: true },
                children: [
                    {
                        name: 'ChatDialog',
                        path: 'chat',
                        component: () => import('../components/ChatDialog.vue')
                    },
                    {
                        name: 'NoteEditorDialog',
                        path: 'note/edit',
                        component: () => import('../components/NoteEditorDialog.vue')
                    },
                    {
                        name: 'NoteDialog',
                        path: 'note/view/:noteId/:comments',
                        component: () => import('../components/NoteDialog.vue')
                    },
                    {
                        name: 'ContentsDialog',
                        path: 'contents/view/',
                        component: () => import('../components/ContentsDialog.vue')
                    },
                    {
                        name: 'PdfDialog',
                        path: 'pdf/view/:sourceIndex',
                        component: () => import('../components/PdfDialog.vue')
                    },
                    {
                        name: 'TeamDialog',
                        path: 'team',
                        component: () => import('../components/TeamDialog.vue')
                    },
                    {
                        name: 'DeleteWorkspaceDialog',
                        path: 'delete',
                        component: () => import('../components/DeleteWorkspaceDialog.vue')
                    }
                ]
            },
        ],
    },

    {
        path: '/auth',
        component: () => import('../layouts/AuthLayout.vue'),
        beforeEnter: async (to, from, next) => {
            // If we have the user, go to the main page.
            if(auth.currentAccount) {
                next('/');
            }
            else {
                next();
            }
        },
        children: [
            {
                name: 'Auth',
                path: '',
                component: () => import('../pages/Auth.vue'),
                meta: { requiresAuth: false }
            }
        ]
    },

    // Always leave this as last one,
    // but you can also remove it
    {
        name: 'Error404',
        path: '/:catchAll(.*)*',
        meta: { is404: true },
        component: () => import('../pages/Error404.vue'),
    },
];

export default routes;
