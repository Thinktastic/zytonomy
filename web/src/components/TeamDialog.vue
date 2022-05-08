<template>
    <QDialog
        position="right"
        full-height
        :maximized="$q.screen.lt.md"
        persistent
        square
        v-model="visible">
        <QCard
            class="column full-height full-width no-wrap"
            :style="$q.screen.lt.md ? `min-width: ${$q.screen.width}px` : 'min-width: 800px'">
            <QToolbar class="bg-white text-dark">
                <QAvatar>
                    <QIcon name="mdi-account-multiple-outline" />
                </QAvatar>
                <QToolbarTitle>Manage Team</QToolbarTitle>
                <QBtn flat round icon="mdi-close" @click="$router.back()"></QBtn>
            </QToolbar>

            <QCardSection class="col column q-px-none">
                <QTabs
                    inline-label
                    class="col-auto text-primary"
                    dense
                    v-model="tab"
                    narrow-indicator
                    :breakpoint="0">
                    <QTab
                        name="active_users"
                        :label="$q.screen.lt.md ? '' : 'Active Users'"
                        icon="mdi-account-outline"/>
                    <QTab
                        name="new_site_user"
                        :label="$q.screen.lt.md ? '' : 'Invite User'"
                        icon="mdi-account-plus-outline"/>
                </QTabs>

                <QSeparator/>

                <QTabPanels
                    class="col"
                    v-model="tab"
                    animated>
                    <!--// Tab panel for the active site users //-->
                    <QTabPanel
                        name="active_users">
                        <QScrollArea
                            style='height: 100%;'
                            :thumb-style="{width: '5px', borderRadius: '2px', right: '1px'}">
                            <h6 class="q-my-xs">Workspace Users</h6>
                            <QList>
                                <QItem
                                    v-for="(member) in workspace?.members"
                                    :key="member.user.id">
                                    <QItemSection avatar>
                                        <QAvatar
                                            rounded
                                            icon="mdi-account-circle-outline">
                                        </QAvatar>
                                    </QItemSection>

                                    <QItemSection>
                                        <QItemLabel class="text-subtitle1">{{ member.user.name }}</QItemLabel>
                                        <QItemLabel caption>{{ dateUtils.formatDateTime(member.addedUtc) }}</QItemLabel>
                                    </QItemSection>

                                    <QItemSection side>
                                        <QBtn flat square label="Manage" color="primary" dense/>
                                    </QItemSection>
                                </QItem>
                            </QList>

                            <QSeparator class="q-my-md"/>

                            <h6 class="q-my-xs">Pending Invitations</h6>

                            <QBanner
                                v-if="!pendingInvites || pendingInvites.length === 0"
                                class="bg-teal-1 text-primary"
                                rounded>
                                <template v-slot:avatar>
                                    <QIcon name="mdi-check"/>
                                </template>
                                There are no pending invitations at the moment.
                            </QBanner>
                            <QList v-else>
                                <QItem
                                    v-for="invitation in pendingInvites"
                                    :key="invitation.id"
                                    class="q-px-none">
                                    <QItemSection>
                                        <QItemLabel><span class="text-subtitle1 text-bold">{{ `${invitation.firstName} ${invitation.lastName}` }}</span></QItemLabel>
                                        <QItemLabel caption lines="2">
                                            <a :href="`mailto:${invitation.email}`">{{ invitation.email }}</a><br/>
                                            By: {{ invitation.invitedBy?.name }}
                                        </QItemLabel>
                                    </QItemSection>

                                    <QItemSection side top>
                                        <QItemLabel>{{ dateUtils.formatDateTime(invitation.createdUtc) }}</QItemLabel>
                                        <QBtn icon="mdi-trash-can-outline" flat size="md" class="q-mt-sm" color="dark" dense label="CANCEL"/>
                                    </QItemSection>
                                </QItem>
                            </QList>
                        </QScrollArea>
                    </QTabPanel>

                    <!--// Tab panel for the active site users //-->
                    <QTabPanel
                        name="new_site_user">
                        Invite a new member to this workspace

                        <QForm
                            ref="registrationForm"
                            class="row q-col-gutter-sm">
                            <QInput
                                :disable="registered"
                                class="col-xs-12"
                                label="Email of the team member to invite"
                                stack-label
                                :clearable="$q.screen.lt.sm"
                                v-model="email"
                                :rules="[
                                    val => !!val || 'Please provide an email',
                                    val => /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/.test(val) || 'Hmm...this doesn\'t look like a valid email.'
                                ]"
                                lazy-rules
                                no-error-icon/>
                            <QInput
                                :disable="registered"
                                class="col-xs-12 col-sm-6"
                                label="First name"
                                stack-label
                                :clearable="$q.screen.lt.sm"
                                v-model="firstName"
                                :rules="[val => !!val || 'Please enter a first name or initial']"
                                lazy-rules
                                no-error-icon/>
                            <QInput
                                :disable="registered"
                                class="col-xs-12 col-sm-6"
                                label="Last name"
                                stack-label
                                :clearable="$q.screen.lt.sm"
                                v-model="lastName"
                                :rules="[val => !!val || 'Please enter a last name or initial']"
                                lazy-rules
                                no-error-icon/>
                            <QInput
                                :disable="registered"
                                class="col-12"
                                label="Invitation message"
                                stack-label
                                type="textarea"
                                counter
                                autogrow
                                maxlength="600"
                                :clearable="$q.screen.lt.sm"
                                v-model="invitationMessage"
                                :rules="[val => !!val || 'Please provide a short invitation message for the recipient']"
                                lazy-rules
                                no-error-icon/>
                            <div
                                class="col-12 text-center">
                                <QBtn
                                    flat
                                    label="Send Invitation"
                                    icon="mdi-email-outline"
                                    :loading="sending"
                                    color="primary"
                                    @click="inviteUser">
                                    <template v-slot:loading>
                                        <QSpinner
                                            color="primary"
                                            :thickness="3"/>
                                    </template>
                                </QBtn>

                                <transition
                                    appear
                                    enter-active-class="animated fadeInDown"
                                    leave-active-class="animated fadeOutUp">
                                    <QBanner
                                        class="bg-green-1 text-green-8 text-left q-mt-md"
                                        rounded
                                        inline-actions
                                        v-if="sentBannerVisible">
                                        <template v-slot:avatar>
                                            <QIcon name="mdi-email-check-outline"/>
                                        </template>
                                        <template v-slot:action>
                                            <QBtn flat label="OK" @click="sentBannerVisible = false"/>
                                        </template>
                                        Invitation sent.
                                    </QBanner>
                                </transition>
                            </div>
                        </QForm>
                    </QTabPanel>
                </QTabPanels>
            </QCardSection>

            <QCardActions class="bg-white">
                <QSpace/>
                <QBtn flat icon="mdi-check-circle-outline" label="Done" color="primary" @click="$router.back()"/>
            </QCardActions>
        </QCard>
    </QDialog>
</template>

<script setup lang="ts">
import { defineComponent, computed, ref, watch } from 'vue';
import { QForm } from 'quasar';
import { useDateUtils }from '../components/shared/dateUtils';

// Model
import { useStore } from '../stores'
import { useRoute } from 'vue-router'
import { Invitation } from '../models/viewModels'

const $store = useStore()
const $route = useRoute()
const dateUtils = useDateUtils()

const visible = computed(() => $route.name == 'TeamDialog');
const pendingInvites = computed(() => $store.data.activeWorkspaceInvitations);
const workspace = computed(() => $store.data.activeWorkspace);

const tab = ref('active_users')

const email = ref('')
const firstName = ref('')
const lastName = ref('')
const invitationMessage = ref('')
const registrationForm = ref<QForm>()
const registering = ref(false)
const registered = ref(false)
const errorMessage = ref('')
const role = ref('')

const sending = ref(false)
const sentBannerVisible = ref(false)

watch(visible, async (newState) => {
    if(!newState) {
        return
    }

    // Load the pending invitations.
    await $store.data.getPendingInvitations();
})

async function inviteUser() {
    const valid = await registrationForm.value?.validate(true)

    if(!valid) {
        return
    }

    sending.value = true

    await $store.app.inviteUser(new Invitation(
        email.value, firstName.value, lastName.value, invitationMessage.value
    ));

    firstName.value = '';
    lastName.value = '';
    email.value = '';
    invitationMessage.value = '';
    registrationForm.value?.resetValidation();
    sentBannerVisible.value = true;

    sending.value = false;
}
</script>
