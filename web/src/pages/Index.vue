<template>
    <QPage class="row" padding>
        <div class="col-xl-6 offset-xl-3 col-lg-8 offset-lg-2 col-md-10 offset-md-1 col-sm-12 q-gutter-md">
            <QCard flat>
                <QCardSection>
                    <div class="text-h6">Create</div>
                </QCardSection>

                <QCardSection>
                    Let's get started by creating a new workspace where you curate your own knowledge base.
                </QCardSection>

                <QSeparator />

                <QCardActions align="center">
                    <QBtn flat :to="{ name: 'WorkspaceCreate' }" color="dark">Get Started</QBtn>
                </QCardActions>
            </QCard>

            <QCard flat>
                <QCardSection>
                    <div class="text-h6">Explore</div>
                </QCardSection>

                <QCardSection>
                    Find curated workspaces by topics you're interested in.
                </QCardSection>

                <QSeparator />

                <QCardActions align="center">
                    <QBtn flat :to="{ name: 'WorkspaceCreate' }" color="dark">Get Started</QBtn>
                </QCardActions>
            </QCard>

            <QCard flat>
                <QCardSection>
                    <div class="text-h6">Join</div>
                </QCardSection>

                <QCardSection v-if="invitations.length === 0">
                    You do not have any pending invitations at the moment.  Create your own workspace or explore existing workspaces.
                </QCardSection>
                <QCardSection v-else class="q-pt-none">
                    <QList>
                        <QItem
                            v-for="invitation in invitations"
                            :key="invitation.id"
                            class="q-px-none">
                            <QItemSection>
                                <QItemLabel><span class="text-subtitle1 text-bold">{{ invitation.name }}</span></QItemLabel>
                                <QItemLabel lines="1">{{ invitation.message }}</QItemLabel>
                                <QItemLabel caption>From: {{ invitation.invitedBy?.name }}</QItemLabel>
                            </QItemSection>

                            <QItemSection side top>
                                <QItemLabel caption>{{ dateUtils.formatDateTime(invitation.createdUtc) }}</QItemLabel>
                                <QBtn
                                    icon="mdi-account-check-outline"
                                    class="q-mt-sm"
                                    flat
                                    size="md"
                                    color="dark"
                                    dense
                                    label="JOIN"
                                    :loading="invitation.status === 'Accepting'"
                                    @click="acceptInvitation(invitation)">
                                    <template v-slot:loading>
                                        <QSpinner
                                            color="primary"
                                            :thickness="3"/>
                                    </template>
                                </QBtn>
                            </QItemSection>
                        </QItem>
                    </QList>
                </QCardSection>
            </QCard>
        </div>
    </QPage>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useStore } from '../stores'
import { GenericRef, Invitation } from '../models/viewModels'
import { useDateUtils } from '../components/shared/dateUtils'
import { onBeforeRouteUpdate, useRouter } from 'vue-router'

const $store = useStore();
const $router = useRouter();
const dateUtils = useDateUtils();

const showGettingStarted = ref(false);

const workspaces = computed<GenericRef[]>(() => $store.data.workspaces || []);
const targetWorkspaceId = computed(() => $store.data.targetWorkspaceId);

const invitations = computed(() => $store.data.userPendingInvitations);

onBeforeRouteUpdate(async (guard) => {
    await $store.data.getUserInvitations();
});

// Watches for changes in the content set; once it is set and it is empty
// we should display the option to create a new content set.
watch(workspaces, () => {
    if (workspaces.value.length > 0) {
        return; // Nothing to do
    }

    // Show a Get Started items
    showGettingStarted.value = true;
})

// It is not possible to access router in Vuex store so we instead listen
// for the state change and then make router change.  After the user accepts
// the invitation, we redirect the user to the workspace.
watch(targetWorkspaceId, async (newValue) => {
    console.log(`targetWorkspaceId VALUE IS: ${newValue}`);

    if(!newValue) {
        return;
    }

    console.log('NAVIGATING TO WORKSPACE');

    await $router.push({ name: 'WorkspaceView', params: { id: targetWorkspaceId.value || '' } });
})

async function acceptInvitation(invitation:Invitation) {
    await $store.data.acceptWorkspaceInvitation(invitation.id);
}
</script>

