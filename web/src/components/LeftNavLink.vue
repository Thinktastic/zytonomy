<template>
    <QItem clickable tag="a" :to="{ name: 'WorkspaceView', params: { id: workspace.id } }">
        <QItemSection avatar>
            <QAvatar>
                <QSpinner
                    v-if="provisioningWorkspaceId == workspace.id"
                    color="black"
                    :thickness="3"/>
                <QIcon
                    v-else
                    name="mdi-cube-outline"
                    size="md"/>
                <QBadge
                    v-if="workspaceEntity?.alertCount && workspaceEntity?.alertCount > 0"
                    color="primary"
                    rounded
                    floating>{{ workspaceEntity?.alertCount }}</QBadge>
            </QAvatar>
        </QItemSection>

        <QItemSection>
            <QItemLabel>{{ workspace.name }}</QItemLabel>
        </QItemSection>
    </QItem>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useStore } from '../stores'
import { GenericRef } from '../models/viewModels'

const props = defineProps<{
    workspace: GenericRef
}>();

const $store = useStore();

const workspaceEntity = computed(
    () => $store.data.loadedWorkspaces.filter(w => w.id === props.workspace.id)[0]
)

const provisioningWorkspaceId = computed<string|null>(
    () => $store.data.provisioningWorkspaceId
)
</script>
