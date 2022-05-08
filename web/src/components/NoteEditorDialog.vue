<template>
    <QDialog
        position="right"
        full-height
        persistent
        :maximized="$q.screen.lt.md"
        square
        v-model="visible"
        @before-hide="clearPendingEntry">
        <QCard class="column full-height full-width no-wrap" :style="$q.screen.lt.md ? `min-width: ${$q.screen.width}px` : 'min-width: 800px'">
            <QToolbar class="bg-white text-dark">
                <QAvatar>
                    <QIcon name="mdi-note-plus-outline" />
                </QAvatar>
                <QToolbarTitle>Add Note</QToolbarTitle>
                <QBtn flat round icon="mdi-close" @click="$router.go(-1)"></QBtn>
            </QToolbar>

            <QToolbar class="text-dark bg-white" v-if="pendingEntry">
                From: <span class="text-weight-bold">{{ sourceOriginalName }}</span>
            </QToolbar>

            <QCardSection class="col col-auto">
                <QInput
                    stack-label
                    label="Title"
                    dense
                    v-model="note.name"
                    :disable="!editable"
                    clearable
                    autofocus></QInput>
                <!--// Toolbar for the editor //-->
                <div class="q-mt-md" v-if="editable">
                    <QBtnGroup outline class="q-mr-sm">
                        <QBtn icon="mdi-format-header-1" text-color="grey-8" @click="editor?.chain().focus().toggleHeading({ level: 1}).run()" dense outline :color="editor?.isActive('heading', { level: 1 }) ? 'grey-4' : 'white' "/>
                        <QBtn icon="mdi-format-header-2" text-color="grey-8" @click="editor?.chain().focus().toggleHeading({ level: 2}).run()" dense outline :color="editor?.isActive('heading', { level: 2 }) ? 'grey-4' : 'white' "/>
                        <QBtn icon="mdi-format-header-3" text-color="grey-8" @click="editor?.chain().focus().toggleHeading({ level: 3}).run()" dense outline :color="editor?.isActive('heading', { level: 3 }) ? 'grey-4' : 'white' "/>
                        <QSeparator vertical/>
                        <QBtn icon="mdi-format-bold" text-color="grey-8" @click="editor?.chain().focus().toggleBold().run()" dense outline :color="editor?.isActive('bold') ? 'grey-4' : 'white' "/>
                        <QBtn icon="mdi-format-italic" text-color="grey-8" @click="editor?.chain().focus().toggleItalic().run()" dense outline :color="editor?.isActive('italic') ? 'grey-4' : 'white' "/>
                        <!--
                            <QBtn icon="mdi-format-underline" text-color="grey-8" @click="editor?.chain().focus().toggleUnderline.run()" dense outline :color="editor?.isActive('underline') ? 'grey-4' : 'white' "/>
                        -->
                        <QBtn icon="mdi-format-quote-close" text-color="grey-8" @click="editor?.chain().focus().toggleBlockquote().run()" dense outline :color="editor?.isActive('blockquote') ? 'grey-4' : 'white' "/>
                        <QSeparator vertical/>
                        <QBtn icon="mdi-format-list-bulleted" text-color="grey-8" @click="editor?.chain().focus().toggleBulletList().run()" dense outline :color="editor?.isActive('bulletList') ? 'grey-4' : 'white' "/>
                        <QBtn icon="mdi-format-list-numbered" text-color="grey-8" @click="editor?.chain().focus().toggleOrderedList().run()" dense outline :color="editor?.isActive('orderedList') ? 'grey-4' : 'white' "/>
                        <!--
                        <QBtn icon="mdi-format-list-checks" text-color="grey-8" @click="editor?.chain().focus().todoList().run()" dense outline :color="editor?.isActive('todoList') ? 'grey-4' : 'white' "/>
                        -->
                    </QBtnGroup>

                    <QBtnGroup outline :class="$q.screen.lt.sm ? 'q-mt-sm' : ''" class="q-mr-sm">
                        <QBtn icon="mdi-table" text-color="grey-8" @click="editor?.chain().focus().insertTable({ rows: 3, cols: 3, withHeaderRow: true}).run()" dense outline color='white'/>
                        <QBtn icon="mdi-table-remove" text-color="grey-8" @click="editor?.chain().focus().deleteTable().run()" dense outline color='white'/>
                        <QSeparator vertical/>
                        <QBtn icon="mdi-table-column-plus-before" text-color="grey-8" @click="editor?.chain().focus().addColumnBefore().run()" dense outline color='white'/>
                        <QBtn icon="mdi-table-column-plus-after" text-color="grey-8" @click="editor?.chain().focus().addColumnAfter().run()" dense outline color='white'/>
                        <QBtn icon="mdi-table-column-remove" text-color="grey-8" @click="editor?.chain().focus().deleteColumn().run()" dense outline color='white'/>
                        <QSeparator vertical/>
                        <QBtn icon="mdi-table-row-plus-after" text-color="grey-8" @click="editor?.chain().focus().addRowBefore().run()" dense outline color='white'/>
                        <QBtn icon="mdi-table-row-plus-before" text-color="grey-8" @click="editor?.chain().focus().addRowAfter().run()" dense outline color='white'/>
                        <QBtn icon="mdi-table-row-remove" text-color="grey-8" @click="editor?.chain().focus().deleteRow().run()" dense outline color='white'/>
                        <QSeparator vertical/>
                        <QBtn icon="mdi-table-merge-cells" text-color="grey-8" @click="editor?.chain().focus().mergeOrSplit().run()" dense outline color='white'/>
                    </QBtnGroup>

                    <QBtnGroup outline :class="$q.screen.lt.sm ? 'q-mt-sm' : ''">
                        <QBtn icon="mdi-video-plus-outline" text-color="grey-8" dense outline color='white' @click="showVideoInput = true"/>
                        <QBtn icon="mdi-file-image-outline" text-color="grey-8" dense outline color='white' @click="showImageInput = true"/>
                    </QBtnGroup>
                </div>
            </QCardSection>

            <QCardSection class="col q-pt-none" style="min-height: 200px" @drop="onFileDrop">
                <QScrollArea style='height: 100%;' :thumb-style="{width: '5px', borderRadius: '2px', right: '1px'}">
                    <!--// Display the note body //-->
                    <BubbleMenu
                        :editor="editor"
                        v-if="editable"
                        v-show="!editor?.isActive('image') && !editor?.isActive('video')">
                        <QBtn icon="mdi-format-color-highlight" round unelevated text-color="white" color="yellow" @click="editor?.chain().focus().toggleHighlight().run()" dense />
                    </BubbleMenu>

                    <!--// The input control for adding an image //-->
                    <transition
                        appear
                        enter-active-class="animated fadeInDown"
                        leave-active-class="animated fadeOutUp">
                        <QCardSection class="col col-auto q-pt-none bg-grey-2"
                            v-if="showImageInput">
                            <QFile
                                v-model="imageFile"
                                label="Pick an image to upload"
                                use-chips
                                dense
                                append>
                                <template v-slot:prepend>
                                    <QIcon name="mdi-file-image-outline" />
                                </template>

                                <template v-slot:after>
                                    <QBtn
                                        flat
                                        icon="mdi-cloud-upload"
                                        class="q-mt-md"
                                        color="dark"
                                        dense
                                        :disable="imageFile == null"
                                        @click="addImage"/>
                                    <QBtn
                                        flat
                                        icon="mdi-close"
                                        class="q-mt-md"
                                        color="dark"
                                        dense
                                        @click="showImageInput = false"/>
                                </template>
                            </QFile>
                        </QCardSection>
                    </transition>

                    <!--// The input control for adding a video //-->
                    <transition
                        appear
                        enter-active-class="animated fadeInDown"
                        leave-active-class="animated fadeOutUp">
                        <QCardSection class="col col-auto q-pt-none bg-grey-2"
                            v-if="showVideoInput">
                            <QInput
                                v-model="videoUrl"
                                type="text"
                                label="Enter a video URL"
                                dense
                                append>
                                <template v-slot:prepend>
                                    <QIcon name="mdi-video-plus-outline" />
                                </template>

                                <template v-slot:after>
                                    <QBtn
                                        flat
                                        icon="mdi-cloud-upload"
                                        class="q-mt-md"
                                        color="dark"
                                        dense
                                        :disable="videoUrl === ''"
                                        @click="addVideo"/>
                                    <QBtn
                                        flat
                                        icon="mdi-close"
                                        class="q-mt-md"
                                        color="dark"
                                        dense
                                        @click="showVideoInput = false"/>
                                </template>
                            </QInput>
                        </QCardSection>
                    </transition>

                    <EditorContent
                        :editor="editor"/>
                </QScrollArea>
            </QCardSection>

            <QCardSection class="col col-auto q-px-md q-py-xs">
                <QSelect
                    stack-label
                    label="Tags (ENTER for each tag)"
                    dense
                    use-input
                    use-chips
                    :disable="!editable"
                    multiple
                    hide-dropdown-icon
                    input-debounce="0"
                    new-value-mode="add"
                    v-model="note.tags"></QSelect>
            </QCardSection>

            <QCardSection class="col col-auto q-gutter-md q-py-sm text-center">
                <QRadio keep-color v-model="note.color" val="orange" color="orange" size="lg" dense :disable="!editable" title="Orange note"/>
                <QRadio keep-color v-model="note.color" val="pink" color="pink" size="lg" dense :disable="!editable" title="Pink note"/>
                <QRadio keep-color v-model="note.color" val="purple" color="purple" size="lg" dense :disable="!editable" title="Purple note"/>
                <QRadio keep-color v-model="note.color" val="indigo" color="indigo" size="lg" dense :disable="!editable" title="Indigo note"/>
                <QRadio keep-color v-model="note.color" val="blue" color="blue" size="lg" dense :disable="!editable" title="Blue note"/>
                <QRadio keep-color v-model="note.color" val="cyan" color="cyan" size="lg" dense :disable="!editable" title="Cyan note"/>
                <QRadio keep-color v-model="note.color" val="teal" color="teal" size="lg" dense :disable="!editable" title="Teal note"/>
                <QRadio keep-color v-model="note.color" val="light-green" color="light-green" size="lg" dense :disable="!editable" title="Light green note"/>
            </QCardSection>

            <QCardSection class="col col-auto" horizontal>
                <QCardSection class="col-6 q-py-xs q-pl-md">
                    <QSelect stack-label label="Importance" v-model="note.importance" dense :options="importanceOptions" :disable="!editable"/>
                </QCardSection>
                <QCardSection class="col-6 q-py-xs q-pr-md">
                    <QSelect
                        stack-label
                        label="Icon"
                        v-model="note.icon"
                        dense
                        emit-value
                        :disable="!editable"
                        :options="iconOptions"
                        options-selected-class="text-primary">
                        <template v-slot:option="scope">
                            <QItem v-bind="scope.itemProps" v-on="scope.focused">
                                <QItemSection avatar>
                                    <QIcon :name="scope.opt.value"/>
                                </QItemSection>
                                <QItemSection>
                                    <QItemLabel>{{ scope.opt.label }}</QItemLabel>
                                </QItemSection>
                            </QItem>
                        </template>
                        <template v-slot:selected-item="scope">
                            <QIcon :name="scope.opt" :color="scope.opt.color"></QIcon>
                        </template>
                    </QSelect>
                </QCardSection>
            </QCardSection>

            <QCardActions class="bg-white">
                <QBtn v-if="editable" flat icon="mdi-cancel" color="grey-8" @click="$router.go(-1)" label="Cancel"/>
                <QBtn v-else flat icon="mdi-check-circle" color="grey-8" @click="$router.go(-1)" label="Done"/>
                <QSpace/>
                <QToggle label="Private note" v-model="note.isPrivate" checked-icon="mdi-lock-outline" :disable="!editable"/>
                <QBtn flat icon="mdi-content-save-outline" color="primary" @click="saveNote" v-if="editable" label="Save"></QBtn>
            </QCardActions>
        </QCard>
    </QDialog>
</template>

<script setup lang="ts">
import { computed, ref, watch, onBeforeUnmount } from 'vue'
import { useEditor, EditorContent, BubbleMenu, Editor } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Table from '@tiptap/extension-table'
import TableRow from '@tiptap/extension-table-row'
import TableCell from '@tiptap/extension-table-cell'
import TableHeader from '@tiptap/extension-table-header'
import Highlight from '@tiptap/extension-highlight'
import Image from '@tiptap/extension-image'
import { Video } from '../components/tiptap/Video'
import { useRoute, useRouter } from 'vue-router'
import { useStore } from '../stores'
import { ContentSource, Note } from '../models/viewModels'

const $route = useRoute()
const $router = useRouter()
const $store = useStore()

const identity = computed(() => $store.app.identity)
const workspace = computed(() => $store.data.activeWorkspace)
const visible = computed(() => $route.name === 'NoteEditorDialog')
const editable = ref(true) // TODO: This may have other purposes
const pendingEntry = computed(() => $store.data.pendingEntry)
const showImageInput = ref(false)
const imageFile = ref<File>()
const showVideoInput = ref(false)
const videoUrl = ref('')

const sourceOriginalName = computed(() => {
    const sourceName = $store.data.pendingEntry?.source || ''

    if(!workspace.value) {
        return sourceName
    }

    const source = workspace.value.sources?.find(s => s.blobStorageFileName?.endsWith(sourceName))

    return source?.displayName || $store.data.pendingEntry?.source
})

// Start editor components
const editor = new Editor({
    content: '',
    editable: editable.value,
    extensions: [
        StarterKit,
        Table,
        TableCell,
        TableRow,
        TableHeader,
        Highlight,
        Image,
        Video
    ]
});

onBeforeUnmount(() => {
    editor.destroy()
})

const note = ref<Note>(new Note(identity.value, workspace.value?.id))

watch(visible, (newVal) => {
    if(!newVal) {
        return
    }

    const newNote = new Note(identity.value, workspace.value?.id)
    newNote.body = pendingEntry.value?.bodyHtml || ''
    newNote.name = $store.data.pendingEntry?.title || ''

    note.value = newNote

    // This implies that whichever source navigates to this route will
    // set pendingEntry first.
    editor.commands.setContent(note.value?.body || '')
})

const importanceOptions = ref(['High', 'Normal', 'Low'])

const iconOptions = ref([
    { value: 'mdi-alert-circle-outline', label: 'Alert - Circle' },
    { value: 'mdi-lock-alert-outline', label: 'Alert - Lock' },
    { value: 'mdi-bell-alert-outline', label: 'Alert - Bell' },
    { value: 'mdi-account-alert-outline', label: 'Alert - User' },
    { value: 'mdi-beaker-outline', label: 'Beaker / Sample' },
    { value: 'mdi-calendar-month-outline', label: 'Calendar' },
    { value: 'mdi-clipboard-check-outline', label: 'Clipboard / Checklist' },
    { value: 'mdi-tooltip-image-outline', label: 'Diagram' },
    { value: 'mdi-heart-outline', label: 'Heart' },
    { value: 'mdi-pencil-circle-outline', label: 'Pencil' },
    { value: 'mdi-ruler', label: 'Ruler' },
    { value: 'mdi-star-outline', label: 'Star' },
    { value: 'mdi-video-outline', label: 'Video' }
])

async function saveNote() {
    if(!note.value) {
        return
    }

    const sourceName = $store.data.pendingEntry?.source || ''

    note.value.body = editor.getHTML() || ''
    note.value.source = workspace.value?.sources?.find(s => s.blobStorageFileName?.endsWith(sourceName)) || new ContentSource()
    note.value.kbEntryId = pendingEntry.value?.kbId || 0

    await $store.data.saveNote(note.value);

    $router.go(-1)
}

function clearPendingEntry() {
    $store.data.setPendingEntry(null);
}

function onFileDrop(e: any) {
    e.stopPropagation()
    e.preventDefault()

    const files = e.dataTransfer.files

    const file = files[0]

    if (!file.type.match('image.*')) {
        return
    }

    const reader = new FileReader()

    reader.onload = (f) => {
        const url = f.target?.result as string

        // Add the image to the editor
        editor.chain().focus().setImage({ src: url }).run()
    }

    // https://aboutweb.dev/blog/tiptap2-vue3-extending-image-functionality/

    reader.readAsDataURL(file)
}

function addImage() {
    // Adds the image into the TipTap editor.
    const reader = new FileReader()

    reader.onload = (f) => {
        const url = f.target?.result as string

        // Add the image to the editor
        editor.chain().focus().setImage({ src: url }).run()
    }

    if (imageFile.value == null) {
        return
    }

    reader.readAsDataURL(imageFile.value.slice())

    imageFile.value = undefined
    showImageInput.value = false
}

function addVideo() {
    let url = videoUrl.value

    // https://www.youtube.com/watch?v=pfuGJk-DVc8
    // https://www.youtube.com/embed/pfuGJk-DVc8

    const regex = /(?:v=|embed\/)(.+)/g
    const videoId = regex.exec(url)?.[1]

    if (!videoId) {
        return
    }

    url = `https://www.youtube.com/embed/${videoId}`

    editor.chain().focus().setVideo({ src: url }).run()

    videoUrl.value = ''
    showVideoInput.value = false
}
</script>

<style>
.iframe__embed {
    width: 100%;
    height: 15rem;
    border: 0;
}

.iframe__input {
    display: block;
    width: 100%;
    font: inherit;
    border: 0;
    border-radius: 5px;
    padding: 0.3rem 0.5rem;
}

/* Styles for tables in the tiptap editor */
.ProseMirror { min-height: 200px; }

.ProseMirror table {
    border-collapse:collapse;
    table-layout:fixed;
    width:100%;
    margin:0;
    overflow:hidden
}
.ProseMirror table td,
.ProseMirror table th {
    min-width:1em;
    border:2px solid #ddd;
    padding:3px 5px;
    vertical-align:top;
    -webkit-box-sizing:border-box;
    box-sizing:border-box;
    position:relative
}
.ProseMirror table td>*,
.ProseMirror table th>* {
    margin-bottom:0
}
.ProseMirror table th {
    font-weight:700;
    text-align:left
}
.ProseMirror table .selectedCell:after {
    z-index:2;
    position:absolute;
    content:"";
    left:0;
    right:0;
    top:0;
    bottom:0;
    background:rgba(200,200,255,.4);
    pointer-events:none
}
.ProseMirror table .column-resize-handle {
    position:absolute;
    right:-2px;
    top:0;
    bottom:0;
    width:4px;
    z-index:20;
    background-color:#adf;
    pointer-events:none
}
.ProseMirror .tableWrapper {
    margin:1em 0;
    overflow-x:auto;
}
.ProseMirror .resize-cursor {
    cursor:ew-resize;
    cursor:col-resize;
}

.ProseMirror img {
    max-width: 100%;
}
</style>