<template>
    <QItem>
        <QItemSection avatar>
            <QAvatar>
                <QIcon
                    color="grey-6"
                    name="mdi-cube-outline"
                    size="md"/>
            </QAvatar>
        </QItemSection>

        <QItemSection>
            <QItemLabel>{{ invitation.name }}</QItemLabel>
        </QItemSection>

        <QItemSection side>
            <QBtn
                icon="mdi-account-check-outline"
                color="dark"
                flat size="sm"
                dense
                :loading="invitation.status === 'Accepting'"
                label="JOIN"
                @click="acceptInvitation(invitation)">
                <template v-slot:loading>
                    <QSpinner
                        color="primary"
                        :thickness="3"/>
                </template>
            </QBtn>
        </QItemSection>
    </QItem>
</template>

<script setup lang="ts">
import { Invitation } from '../models/viewModels'
import { useStore } from '../stores'

const props = defineProps<{
    invitation: Invitation
}>();

const $store = useStore()

async function acceptInvitation(invitation:Invitation) {
    await $store.data.acceptWorkspaceInvitation(invitation.id);
}
</script>
