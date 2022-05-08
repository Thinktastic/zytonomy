<template>
    <QPage padding class="row items-stretch q-pb-none">
        <div class="column full-width">
            <!--// Header //-->
            <div class="col col-auto">
                <QToolbar
                    class="q-px-none">
                    <QToolbarTitle>
                        {{ workspace?.name }}
                    </QToolbarTitle>
                    <QBtn
                        icon="mdi-comment-question"
                        flat
                        :dense="$q.screen.lt.md"
                        :class="$q.screen.lt.md ? 'q-px-xs' : ''"
                        color="dark"
                        size="md">
                        <QPopupEdit
                            v-model="workspaceQuestion"
                            v-slot="scope"
                            ref="questionPopup"
                            @save="askQuestion"
                            label-set="ASK"
                            buttons
                            title="How can I help?"
                            self="bottom start"
                            :cover="false"
                            max-width="360px"
                            style="width: 360px">
                            <QInput
                                dense
                                autofocus
                                clearable
                                clear-icon="mdi-backspace-outline"
                                v-model="scope.value"
                                @keyup.enter="scope.set"></QInput>
                        </QPopupEdit>
                    </QBtn>
                </QToolbar>

                <QItem
                    v-if="$q.screen.gt.sm"
                    class="q-px-none q-pt-none">
                    <QItemSection>
                        <QItemLabel lines='2' class='text-weight-light q-pr-xs'>
                            {{ workspace?.description }}
                        </QItemLabel>
                    </QItemSection>
                </QItem>
            </div>

            <QTabPanels
                class="col"
                v-model="tab"
                animated
                @transition="async () => await loadTabContents(tab)">
                <QTabPanel
                    name="notes_tab"
                    class="column bg-grey-2 q-px-none q-py-none">
                    <!--// Search //-->
                    <NoteSearch v-if="activeWorkspaceNoteCount > 0" />

                    <!--// Welcome note if nothing here yet //-->
                    <div v-else class="row">
                        <QChatMessage
                            class="text-h5 q-mt-lg col-md-8 offset-md-2 col-sm-12 offset-sm-0"
                            bg-color="white"
                            text-color="grey-6">
                            <template v-slot:avatar>
                                <QAvatar
                                    icon="mdi-blur"
                                    size="72px"
                                    color="primary"
                                    text-color="white"
                                    class="q-mx-sm" square rounded>
                                </QAvatar>
                            </template>

                            <template v-slot:default>
                                <div class="text-center q-mt-sm">
                                    There's nothing here yet.  Get started by adding a note or asking a question.  I'm here to help.<br/>
                                    <QBtn icon="mdi-note-plus-outline" size="lg" color="primary" rounded flat class="q-mt-md" :to="{ name: 'NoteEditorDialog' }" append> ADD A NOTE</QBtn>
                                    <QBtn icon="mdi-comment-question" size="lg" color="primary" rounded class="q-ml-md q-mt-md" flat :to="{ name: 'ChatDialog' }" append> ASK A QUESTION</QBtn>
                                </div>
                            </template>
                        </QChatMessage>
                    </div>

                    <!--// Cards //-->
                    <div class="col">
                        <QScrollArea
                            style='height: 100%;'
                            :thumb-style="{width: '5px', borderRadius: '2px', right: '1px'}">
                            <div v-if="notes && notes.length > 0"
                                key="note-cards"
                                class="row q-col-gutter-sm q-pt-md">
                                <div
                                    v-for="(note, index) in notes"
                                    :key="`${index}_${note.id}`"
                                    class='col-12 col-sm-6 col-md-4 col-lg-3'>
                                    <transition
                                        appear
                                        enter-active-class="animated fadeInDown"
                                        leave-active-class="animated fadeOutUp">
                                        <NoteCard
                                            :note="note"
                                            :viewing="false"
                                            @view-note="toggleNoteDialog"
                                            @view-comments="toggleNoteDialogComments"/>
                                    </transition>
                                </div>
                            </div>

                            <!--// Filters are hiding results //-->
                            <div v-else-if="activeWorkspaceNoteCount > 0"
                                key="no-results"
                                class="row">
                                <QChatMessage
                                    class="text-h5 q-mt-lg col-md-8 offset-md-2 col-sm-12 offset-sm-0"
                                    bg-color="grey-3"
                                    text-color="grey-6">
                                    <template v-slot:avatar>
                                        <QAvatar
                                            icon="mdi-blur"
                                            size="72px"
                                            color="primary"
                                            text-color="white"
                                            class="q-mx-sm" square rounded>
                                        </QAvatar>
                                    </template>

                                    <template v-slot:default>
                                        <div class="text-center q-mt-sm">
                                            There's nothing here; try changing your filter criteria.

                                            <QBtn size="lg" color="primary" rounded class="q-ml-md q-mt-lg" flat label="RESET FILTERS" icon="mdi-backspace-outline" @click="resetFilters"/>
                                        </div>
                                    </template>
                                </QChatMessage>
                            </div>
                        </QScrollArea>
                    </div>
                </QTabPanel>
            </QTabPanels>
        </div>

        <!--// Dialogs //-->
        <ChatDialog/>
        <NoteEditorDialog/>
        <NoteDialog/>
        <ContentsDialog/>
        <PdfDialog/>
        <TeamDialog/>
        <DeleteWorkspaceDialog/>
    </QPage>
</template>

<script setup lang="ts">
import { computed, onBeforeMount, ref, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useStore } from '../stores'
import { Filter, GenericRef, Message, MessageType, Note, Workspace } from '../models/viewModels'
import ChatDialog from '../components/ChatDialog.vue'
import NoteEditorDialog from '../components/NoteEditorDialog.vue'
import NoteCard from '../components/NoteCard.vue'
import NoteSearch from '../components/NoteSearch.vue'
import NoteDialog from '../components/NoteDialog.vue'
import ContentsDialog from '../components/ContentsDialog.vue'
import PdfDialog from '../components/PdfDialog.vue'
import TeamDialog from '../components/TeamDialog.vue'
import DeleteWorkspaceDialog from '../components/DeleteWorkspaceDialog.vue'
import { QPopupEdit } from 'quasar'
import { plainToInstance } from 'class-transformer'

import md5 from 'md5'
import dayjs from 'dayjs'
import { TabType } from '../stores/appStore'

const $store = useStore()
const $router = useRouter()
const $route = useRoute()

const routeParams = computed(() => $route.params);
const identity = computed(() => $store.app.identity)
const workspace = computed<Workspace | null>(() => $store.data.activeWorkspace)

const questionPopup = ref<QPopupEdit>()
const workspaceQuestion = ref('')
const tab = computed(() => $store.app.workspaceTab)

const activeWorkspaceNoteCount = computed(() => $store.data.activeWorkspaceNotes.length || 0)
const removedWorkspaceId = computed(() => $store.data.removedWorkspaceId)

onBeforeMount(async () => {
    // This gets triggered on the first load.
    console.log(`MOUNTED; LOADING WORKSPACE: ${routeParams.value.id}`);
    await $store.data.loadWorkspace(routeParams.value.id as string)
    await $store.data.loadWorkspaceNotes(routeParams.value.id as string)
})

watch(routeParams, async (newVal) => {
    // This gets triggered when the route changes subsequently.
    if(!(newVal && newVal.id)) {
        return;
    }

    console.log(`WORKSPACE ROUTE CHANGED: ${newVal.id}`);
    await $store.data.loadWorkspace(newVal.id as string)
    await $store.data.loadWorkspaceNotes(newVal.id as string)
})

watch(removedWorkspaceId, async (newVal) => {
    if (newVal !== workspace.value?.id) {
        return
    }

    // Route home
    await $router.replace({ name: 'Home' })
})

const notes = computed(() => {
    if ($store.data.filters === null || $store.data.filters.length === 0) {
        // No filters
        return $store.data.activeWorkspaceNotes
    }
    else {
        let sort:Filter = { type: 'sort', label: 'color', value: 'ascending' }

        // Iterate each message
        const filteredNotes = $store.data.activeWorkspaceNotes.filter(m => {
            // Iterate each filter and apply logic by type.
            return $store.data.filters.every(f => {
                if (f.type === 'tag') {
                    return m.tags?.includes(f.value)
                }
                else if (f.type === 'color') {
                    return m.color === f.value
                }
                else if (f.type === 'user') {
                    return m.author.id === f.value
                }
                else if (f.type === 'icon') {
                    return m.icon === f.value
                }
                else if (f.type === 'sort') {
                    sort = f

                    return true // Always return true since this is not a filter
                }
            })
        })

        if (filteredNotes) {
            const property = sort.label.toLowerCase()

            // In place sort.
            filteredNotes.sort((a, b) => {
                const aRef = Reflect.get(a, property)
                const bRef = Reflect.get(b, property)

                const ascending = sort.value === 'ascending'

                if (aRef < bRef) { return ascending ? 1 : -1 }
                if (aRef > bRef) { return ascending ? -1 : 1 }
                return 0
            })
        }

        return filteredNotes
    }
});

async function toggleNoteDialog(note: Note) {
    await $router.push({ name : 'NoteDialog', params: { noteId: note.id, comments: 'top' } });
};

async function toggleNoteDialogComments(note: Note) {
    await $router.push({ name : 'NoteDialog', params: { noteId: note.id, comments: 'comments' } });
};

async function resetFilters() {
    await $store.data.clearFilters();
};

async function askQuestion(value: string) {
    console.log(`QUESTION: ${value} in ${$store.data.activeWorkspace?.id}`);

    if(!value || value === '') {
        return;
    }

    const message: Message = plainToInstance(Message, {
        id: md5(`${Date.now().toString()}${identity.value?.id}`),
        workspaceId: $store.data.activeWorkspace?.id || '',
        createdUtc: dayjs.utc().format(),
        author: identity.value?.asRef || new GenericRef(),
        body: value,
        posted: false,
        messageType: MessageType.USER_QUESTION
    });

    await $router.push({ name: 'ChatDialog' });

    await $store.messaging.sendQuestion(message);
};

async function loadTabContents(newVal: TabType) {
    // Workspace tabs in the future.
};

function switchTab(tabName: TabType) {
    $store.app.setWorkspaceTab(tabName);
};
</script>
