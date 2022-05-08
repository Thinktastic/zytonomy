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
                    <QIcon name="mdi-file-cabinet" />
                </QAvatar>
                <QToolbarTitle>Manage Workspace Contents</QToolbarTitle>
                <QBtn flat round icon="mdi-close" @click="$router.back()"></QBtn>
            </QToolbar>

            <QToolbar class="justify-center">
                <QBtn
                    flat
                    color="dark"
                    icon="mdi-file-plus-outline"
                    label="Add Document"
                    @click="showFileInput = true"></QBtn>
            </QToolbar>

            <transition
                appear
                enter-active-class="animated fadeInDown"
                leave-active-class="animated fadeOutUp">
                <QCardSection class="col col-auto"
                    v-if="showFileInput">
                    <QFile
                        v-model="files"
                        label="Pick one or more PDF files"
                        use-chips
                        multiple
                        append
                        clear-icon="mdi-close">
                        <template v-slot:prepend>
                            <QIcon name="mdi-paperclip" />
                        </template>

                        <template v-slot:after>
                            <QBtn
                                flat
                                icon="mdi-cloud-upload"
                                color="dark"
                                dense
                                :disable="files.length === 0"
                                @click="uploadFiles"/>
                            <QBtn
                                flat
                                icon="mdi-close"
                                color="dark"
                                dense
                                @click="showFileInput = false"/>
                        </template>
                    </QFile>
                </QCardSection>
            </transition>

            <QCardSection class="col q-px-none">
                <QList>
                    <QItem
                        v-for="(content, index) in workspace?.sources"
                        :key="contentMd5(content.blobStorageFileName || '')">
                        <QItemSection avatar>
                            <QSpinner
                                v-if="content.status === 'Publishing' || content.status === 'Deleting'"
                                size="lg"
                                class="q-mx-md"
                                :color="content.status === 'Publishing' ? 'primary' : 'red-9' "
                                :thickness="3"/>
                            <QBtn
                                v-else
                                icon="mdi-file-pdf-box"
                                size="xl"
                                flat
                                color="grey-8"
                                :to="{ name: 'PdfDialog', params: { sourceIndex: index } }"
                                append>
                            </QBtn>
                        </QItemSection>

                        <QItemSection>
                            <!--// TODO: PLACEHOLDER //-->
                            <QItemLabel class="text-subtitle1 text-bold">
                                {{ content.displayName }}
                            </QItemLabel>
                            <QItemLabel>Uploaded {{ contentAdded(content.addedUtc || '') }}</QItemLabel>
                            <QItemLabel class="text-caption">{{ contentAuthorName(content.addedById || '') }}</QItemLabel>
                        </QItemSection>

                        <QItemSection side>
                            <QBtn
                                v-if="content.status !== 'Publishing'"
                                flat
                                square
                                label="Manage"
                                color="primary">
                                <QMenu
                                    cover
                                    anchor="top left">
                                    <QList>
                                        <QItem clickable v-close-popup>
                                            <QItemSection avatar class="q-mr-none">
                                                <QIcon name="mdi-file-document-edit-outline"/>
                                            </QItemSection>
                                            <QItemSection>
                                                Edit
                                            </QItemSection>
                                        </QItem>
                                        <QItem clickable v-close-popup>
                                            <QItemSection avatar class="q-mr-none">
                                                <QIcon name="mdi-file-replace-outline"/>
                                            </QItemSection>
                                            <QItemSection>
                                                Replace
                                            </QItemSection>
                                        </QItem>
                                        <QItem clickable v-close-popup @click="deleteSource(index)">
                                            <QItemSection avatar class="q-mr-none">
                                                <QIcon name="mdi-delete-outline"/>
                                            </QItemSection>
                                            <QItemSection>
                                                Delete
                                            </QItemSection>
                                        </QItem>
                                    </QList>
                                </QMenu>
                            </QBtn>
                        </QItemSection>
                    </QItem>
                </QList>
            </QCardSection>

            <QCardActions class="bg-white">
                <QSpace/>
                <QBtn flat icon="mdi-check-circle-outline" label="Done" color="primary" @click="$router.back()"/>
            </QCardActions>
        </QCard>
    </QDialog>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import md5 from 'md5'
import dayjs from 'dayjs'

// Model
import { useStore } from '../stores'
import { useRoute } from 'vue-router'

const $store = useStore()
const $route = useRoute()

const visible = computed(() => $route.name === 'ContentsDialog')
const workspace = computed(() => $store.data.activeWorkspace)

const showFileInput = ref(false)
const files = ref<File[]>([])

function contentMd5(blobName: string): string {
    return md5(blobName)
}

function contentAdded(addedUtc: string): string {
    return dayjs().to(addedUtc)
}

function contentAuthorName(addedById: string): string {
    return workspace.value?.members?.find(m => m.user.id === addedById)?.user.name || ''
}

async function uploadFiles() {
    await $store.data.addDocumentSource(files.value);

    files.value.splice(0, files.value.length)

    showFileInput.value = false
}

async function deleteSource(index: number) {
    await $store.data.deleteSource(index);
}
</script>