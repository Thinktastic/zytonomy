<template>
    <QPage padding class="row">
        <div class="col-xl-6 offset-xl-3 col-lg-8 offset-lg-2 col-md-10 offset-md-1 col-sm-12">
            <div class="text-h4 q-mb-sm">Create New Workspace</div>
            <p>Create a new workspace by providing the source documents for your knowledgebase.</p>

            <div class="text-h6 q-mt-lg">Name and Description</div>

            <QInput
                v-model="title"
                label="Name"
                :rules="[val => !!val || 'Please provide a name for the workspace.']"
                no-error-icon
                stack-label/>

            <QInput
                v-model="description"
                type="textarea"
                label="Description"
                counter
                maxlength="300"
                autogrow
                :rules="[val => !!val || 'Please provide a description for the workspace.']"
                no-error-icon
                stack-label/>

            <div class="text-h6 q-mt-lg">Documents</div>

            <QFile
                v-model="files"
                label="Pick one or more PDF files from your device"
                use-chips
                multiple
                append
                clear-icon="mdi-close">
                <template v-slot:prepend>
                    <QIcon name="mdi-paperclip" />
                </template>
            </QFile>

            <!-- Future Connectors
            <div class="row q-mt-md justify-center">
                <QBtn
                    icon="mdi-google-drive"
                    label="Google Drive"
                    color="dark"
                    flat
                    size="md"/>
                <QBtn
                    icon="mdi-microsoft-onedrive"
                    label="OneDrive"
                    color="dark"
                    flat
                    size="md"/>
                <QBtn
                    icon="mdi-dropbox"
                    label="DropBox"
                    color="dark"
                    flat
                    size="md"/>
                <QBtn
                    icon="mdi-microsoft-sharepoint"
                    label="SharePoint"
                    color="dark"
                    flat
                    size="md"/>
            </div>
            -->

            <QBtn
                class="full-width q-mt-lg"
                label="Create Workspace"
                :disable="!validated"
                :loading="creating"
                @click="createWorkspace">
                <template v-slot:loading>
                    <QSpinner
                        color="primary"
                        :thickness="3"/>
                </template>
            </QBtn>
        </div>
    </QPage>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useStore } from '../stores'
import { WorkspaceDefinition } from '../models/viewModels'

const $store = useStore();
const title = ref('')
const description = ref('')
const files = ref<File[]>([])
const creating = ref(false)
const created = ref(false)

const validated = computed<boolean>(
    () => files.value != null
        && files.value.length !== 0
        && title.value.length > 0
        && description.value.length > 0
)

const provisioningWorkspaceId = computed<string|null>(
    () => $store.data.provisioningWorkspaceId
)

watch(provisioningWorkspaceId, (newVal, prevVal) => {
    // If the provisioning workspace ID gets reset, we are done.
    if(newVal == null && prevVal != null) {
        creating.value = false
        created.value = true
    }
})

async function createWorkspace() {
    const workspace:WorkspaceDefinition = {
        title: title.value,
        description: description.value,
        files: files.value
    }

    creating.value = true

    await $store.data.createWorkspace(workspace);
}
</script>
