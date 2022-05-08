<template>
    <QDialog
        position="right"
        full-height
        :maximized="$q.screen.lt.md"
        square
        persistent
        v-model="visible">
        <QCard
            class="column full-height full-width no-wrap"
            :style="$q.screen.lt.md ? `min-width: ${$q.screen.width}px` : 'min-width: 800px'">
            <QToolbar class="bg-white text-dark">
                <QAvatar>
                    <QIcon name="mdi-cube-off-outline" />
                </QAvatar>
                <QToolbarTitle>Delete Workspace</QToolbarTitle>
                <QBtn flat round icon="mdi-close" @click="$router.back()"></QBtn>
            </QToolbar>

            <QCardSection class="col">
                <QBanner class="text-white bg-red-9">
                    You are about to delete this workspace and the contents associated with it.  This action is not reversible.
                    No contents of the workspace will be saved and no backup will be available.
                    <br/><br/>
                    To continue, type the full name of the workspace <b>{{ workspace?.name }}</b> in the textbox below (case insensitive) to enable the
                    delete button.
                </QBanner>
                <QInput
                    :label="`Type: ${workspace?.name}`"
                    v-model="typedWorkspaceName"
                    class="q-mt-md"
                    color="grey-6"
                    autofocus/>
                <QBtn
                    icon="mdi-cube-off-outline"
                    class="q-mt-md full-width"
                    color="red-9"
                    :disable="typedWorkspaceName.toLowerCase() !== workspace?.name?.toLowerCase()"
                    flat
                    :label="`Delete ${workspace?.name}`"
                    @click="deleteWorkspace"/>
            </QCardSection>

            <QCardActions class="bg-white">
                <QSpace/>
                <QBtn flat icon="mdi-check-circle-outline" label="Cancel" color="primary" @click="$router.back()"/>
            </QCardActions>
        </QCard>
    </QDialog>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'

// Model
import { useStore } from '../stores'
import { useRoute } from 'vue-router'

const $store = useStore()
const $route = useRoute()

const visible = computed(() => $route.name === 'DeleteWorkspaceDialog')
const workspace = computed(() => $store.data.activeWorkspace)

const typedWorkspaceName = ref('')

async function deleteWorkspace() {
    await $store.data.deleteWorkspace(workspace.value?.id || '');
}
</script>