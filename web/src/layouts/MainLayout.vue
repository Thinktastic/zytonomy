<template>
    <div class="constrained bg-grey-2">
        <QLayout view="hHR lpr lfr">
            <!--// Header //-->
            <QHeader bordered class="bg-white text-primary">
                <QToolbar>
                    <QBtn flat round dense icon="mdi-menu" class="q-mr-sm" @click="leftDrawerOpen = !leftDrawerOpen" />
                    <QToolbarTitle>
                        <a :to="{ name: 'Home' }" href="#">
                            <img src="../../public/icons/thinktastic-text-149x32.png" style="vertical-align: middle" />
                        </a>
                    </QToolbarTitle>

                    <QBtn v-if="!fullscreen" round flat icon="mdi-chevron-up" class="q-mr-xs" @click="enterFullscreen">
                    </QBtn>
                    <QBtn v-else round flat icon="mdi-chevron-down" class="q-mr-xs" @click="exitFullscreen">
                    </QBtn>
                    <QBtn round outline>
                        <QAvatar size="26px">
                            {{ identity.monogram }}
                        </QAvatar>
                        <QTooltip>Account</QTooltip>
                    </QBtn>
                </QToolbar>
            </QHeader>

            <!--// Left drawer //-->
            <QDrawer show-if-above v-model="leftDrawerOpen" @before-hide="leftDrawerOpen = false"
                content-class="bg-white" :width="320">
                <QList class="column justify-between full-height">
                    <!--// Current user //-->
                    <div class="col">
                        <QItem>
                            <QItemSection avatar>
                                <QAvatar rounded size="48px">
                                    <QIcon name="mdi-account-circle-outline" size="xl" />
                                </QAvatar>
                            </QItemSection>

                            <QItemSection>
                                <QItemLabel class="caption text-weight-light text-grey-8">Hello,</QItemLabel>
                                <QItemLabel class="text-h6 text-weight-medium text-grey-10">{{ identity.displayName }}
                                </QItemLabel>
                            </QItemSection>
                        </QItem>


                        <!--// List of Workspaces //-->
                        <LeftNavLink v-for="workspace in workspaces" :key="workspace.id" :workspace="workspace" />

                        <InvitationNavLink v-for="invitation in invitations" :key="invitation.id"
                            :invitation="invitation" />

                        <QItem clickable tag="a" :to="{ name: 'WorkspaceCreate' }">
                            <QItemSection avatar>
                                <QAvatar>
                                    <QIcon name="mdi-plus-circle-outline" size="md" />
                                </QAvatar>
                            </QItemSection>

                            <QItemSection>
                                <QItemLabel>Create Workspace</QItemLabel>
                            </QItemSection>
                        </QItem>
                    </div>

                    <div class="col col-auto">
                        <QItem clickable tag="a" href="#">
                            <QItemSection avatar>
                                <QAvatar>
                                    <QIcon name="mdi-cog" color="primary" />
                                </QAvatar>
                            </QItemSection>

                            <QItemSection>
                                <QItemLabel>Settings</QItemLabel>
                            </QItemSection>
                        </QItem>

                        <QItem clickable @click="logout">
                            <QItemSection avatar>
                                <QAvatar>
                                    <QIcon name="mdi-logout-variant" color="primary" />
                                </QAvatar>
                            </QItemSection>

                            <QItemSection>
                                <QItemLabel>Sign Out</QItemLabel>
                            </QItemSection>
                        </QItem>
                    </div>
                </QList>
            </QDrawer>

            <!--// Footer available only on the workspace view //-->
            <transition appear enter-active-class="animated fadeInUp" leave-active-class="animated fadeOutDown">
                <QFooter v-if="viewingWorkspace" class="bg-grey-2 text-primary text-center">
                    <QBtn flat stack color="primary" label="Add" icon="mdi-note-plus-outline"
                        :to="{ name: 'NoteEditorDialog' }" append class="nav-btn" />
                    <QBtn flat stack color="primary" label="Ask" icon="mdi-comment-question"
                        :to="{ name: 'ChatDialog' }" append class="nav-btn" />
                    <QBtn flat stack color="primary" label="View" icon="mdi-file-eye-outline"
                        :to="{ name: 'ContentsDialog' }" append class="nav-btn" />
                    <QBtn flat stack color="primary" label="Manage" icon="mdi-application-cog" class="nav-btn">
                        <QMenu square anchor="top end" self="bottom end" auto-close>
                            <QList class="text-grey-8">
                                <QItem clickable :to="{ name: 'DeleteWorkspaceDialog' }" append>
                                    <QItemSection avatar>
                                        <QIcon name="mdi-cube-off-outline" />
                                    </QItemSection>
                                    <QItemSection>
                                        <QItemLabel>Delete Workspace</QItemLabel>
                                    </QItemSection>
                                </QItem>
                                <!--
                                <QItem clickable>
                                    <QItemSection avatar>
                                        <QIcon name="mdi-package-variant-closed"/>
                                    </QItemSection>
                                    <QItemSection>
                                        <QItemLabel>Archive Settings</QItemLabel>
                                    </QItemSection>
                                </QItem>
                                <QItem clickable>
                                    <QItemSection avatar>
                                        <QIcon name="mdi-tune"/>
                                    </QItemSection>
                                    <QItemSection>
                                        <QItemLabel>Appearance Options</QItemLabel>
                                    </QItemSection>
                                </QItem>
                                -->
                                <!--
                                <QItem clickable>
                                    <QItemSection avatar>
                                        <QIcon name="mdi-graph-outline"/>
                                    </QItemSection>
                                    <QItemSection>
                                        <QItemLabel>KB</QItemLabel>
                                    </QItemSection>
                                </QItem>
                                -->
                                <QItem clickable :to="{ name: 'TeamDialog' }" append>
                                    <QItemSection avatar>
                                        <QIcon name="mdi-account-multiple-outline" />
                                    </QItemSection>
                                    <QItemSection>
                                        <QItemLabel>Team</QItemLabel>
                                    </QItemSection>
                                </QItem>
                            </QList>
                        </QMenu>
                    </QBtn>
                </QFooter>
            </transition>

            <!--// Right drawer //-->
            <!--
            <QDrawer
                show-if-above
                side="right"
                content-class="bg-white"
                :value="rightDrawerOpen"
                :width="200"
                elevated>

            </QDrawer>
            -->

            <!--// Main content //-->
            <QPageContainer class="bg-grey-2">
                <router-view />
            </QPageContainer>
        </QLayout>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, inject, watch } from 'vue'
import { useQuasar } from 'quasar'
import { useStore } from '../stores'
import { useRoute, useRouter } from 'vue-router'
import { MsalWrapper } from '../boot/msal'
import { GenericRef, Identity } from '../models/viewModels'
import LeftNavLink from '../components/LeftNavLink.vue'
import InvitationNavLink from '../components/InvitationNavLink.vue'
import 'reflect-metadata' // See: https://docs.typestack.community/class-transformer/v/develop/01-getting-started
import { Notify } from 'quasar'

const $q = useQuasar()
const $store = useStore()
const $route = useRoute()
const leftDrawerOpen = ref(false)
const rightDrawerOpen = computed(() => ($store.app.rightDrawerOpen))
const viewingWorkspace = computed(() => $route.name === 'WorkspaceView')
const invitations = computed(() => $store.data.userPendingInvitations)
const fullscreen = ref(false)

const $router = useRouter()

const msal = inject<MsalWrapper>('msal')

const identity = computed<Identity>(
    () => $store.app.identity || new Identity()
)

const workspaces = computed<GenericRef[]>(
    () => $store.data.workspaces || []
)

function logout() {
    msal?.logout()
}

async function enterFullscreen() {
    try {
        await $q.fullscreen.request()
        fullscreen.value = true
    }
    catch (error) {
        // No need; we just don't go full screen on unsupported browser.
    }
}

async function exitFullscreen() {
    if (!fullscreen.value) {
        return
    }

    try {
        await $q.fullscreen.exit()
        fullscreen.value = false
    }
    catch (error) {
        // Couldn't exit full screen; likely not in fullscreen.
    }
}
</script>

<style type="text/css">
@import url('https://fonts.googleapis.com/css2?family=Roboto:wght@100;300&display=swap');

.constrained {
    width: 100%;
    height: 100%;
}

.constrained .QLayout,
.constrained .QHeader,
.constrained .QFooter {
    margin: 0 auto;
    /* max-width: 1245px !important; */
}

.nav-btn {
    min-width: 90px;
}
</style>