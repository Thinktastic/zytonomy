<template>
    <QChatMessage
        :name="author?.name"
        :stamp="messageAgeDisplay"
        :text="[]"
        sent
        text-color="black"
        bg-color="grey-3"
        :size="$q.screen.lt.md ? '9' : '8'">
        <template v-slot:avatar>
            <QAvatar
                size="48px"
                class="q-mx-sm" square rounded
                color="primary"
                text-color="white"
                icon="mdi-blur">
            </QAvatar>
        </template>

        <template v-slot:default>
            <div>
                <!--// Response from the service endpoint //-->
                <QTabs
                    v-if="contents && contents.length > 1"
                    inline-label
                    class="text-primary"
                    dense
                    v-model="tab"
                    narrow-indicator
                    :breakpoint="0">
                    <QTab
                        v-for="(entry, index) in contents"
                        :name="`${index}`"
                        :key="`tab_${index}`"
                        :label="`${$q.screen.lt.md ? '' : 'Answer'} ${index + 1}`"
                        :icon="`mdi-hexagon-slice-${entry.relevance}`">
                    </QTab>
                </QTabs>

                <QSeparator v-if="contents && contents.length > 1"/>

                <QTabPanels
                    v-model="tab"
                    animated>
                    <QTabPanel
                        v-for="(entry, index) in contents"
                        :key="`tab_panel_${index}`"
                        :name="`${index}`"
                        class="bg-grey-3">

                        <div class="text-subtitle2 q-mb-md">{{ entry.title }}</div>

                        <!--// This is the view for the response from the message //-->
                        <div v-html="entry.bodyHtml" class="message-body-html"></div>

                        <div v-if="entry.kbId !== 0">
                            <QChip
                                outline
                                square
                                dense
                                color="dark"
                                size="md"
                                clickable
                                @click="$router.push({ name: 'PdfDialog', params: { sourceIndex: index } })">
                                <QAvatar
                                    color="dark"
                                    text-color="white"
                                    icon="mdi-file-pdf-outline"/>
                                <span v-html="getSource(entry)"></span>
                            </QChip>
                        </div>

                        <!--// The bottom actions on the message; relevance 0 indicates it's the no-match response //-->
                        <div class="text-right" v-if="$q.screen.lt.md">
                            <QBtn v-if="entry.relevance !== 0" icon="mdi-note-plus-outline" size="md" color="primary" flat @click="copyMessageToSave"></QBtn>
                            <QBtn icon="mdi-account-question" size="md" color="primary" class="q-ml-xs" flat></QBtn>
                            <QBtn v-if="entry.relevance !== 0" icon="mdi-thumb-down" size="md" color="red" class="q-ml-xs" flat>
                                <QPopupEdit
                                    self="bottom right"
                                    :cover="false"
                                    v-model="popup"
                                    max-width="300px"
                                    label-set="SAVE"
                                    buttons
                                    title="Help Me Get Smarter"
                                    @save="saveSuggestion"
                                    :validate="validateSuggestion">
                                    <span>I didn't find the right information but you can teach me by telling me what question this response <i>would</i> have answered instead.</span>
                                    <QInput dense autofocus v-model="popup"></QInput>
                                </QPopupEdit>
                            </QBtn>
                        </div>
                        <div class="text-right q-mt-sm" v-else>
                            <QBtn v-if="entry.relevance !== 0" size="md" color="primary" flat @click="copyMessageToSave">Note</QBtn>
                            <QBtn size="md" color="primary" class="q-ml-xs" flat>
                                Contact
                            </QBtn>
                            <QBtn v-if="entry.relevance !== 0" size="md" color="red" class="q-ml-xs" flat>
                                Not Useful
                                <QPopupEdit
                                    self="bottom right"
                                    :cover="false"
                                    v-model="popup"
                                    max-width="300px"
                                    label-set="SAVE"
                                    title="Help Me Get Smarter"
                                    buttons
                                    @save="saveSuggestion"
                                    :validate="validateSuggestion">
                                    <span>You can teach me by telling me what question this response <i>would</i> have answered instead.</span>
                                    <QInput dense autofocus v-model="popup"></QInput>
                                </QPopupEdit>
                            </QBtn>
                        </div>

                        <!--// The followup questions which are mapped to this response //-->
                        <div v-if="entry.prompts && entry.prompts.length > 0">
                            <QSeparator/>
                            <p class="q-my-md">I can help with these followups:</p>
                            <QChip
                                outline
                                v-for="prompt in entry.prompts"
                                :key="`prompt_${prompt.qnaId}`"
                                icon="mdi-help-circle-outline"
                                :label="prompt.displayText"
                                clickable
                                @click="askFollowup(prompt)"/>
                        </div>
                    </QTabPanel>
                </QTabPanels>
            </div>
        </template>
    </QChatMessage>
</template>

<script setup lang="ts">
import { useStore } from '../stores'
import { useRouter } from 'vue-router'
import { computed, ref } from 'vue'
import md5 from 'md5'
import dayjs from 'dayjs'
import { GenericRef, Message, MessageType } from '../models/viewModels'
import { KbEntry, KbFollowupPrompt, KbResponse } from '../models/kbModels'
import { marked } from 'marked'

const props = defineProps<{
    message: Message
}>();

const $store = useStore()
const $router = useRouter()

const popup = ref();
const identity = computed(() => $store.app.identity)
const workspace = computed(() => $store.data.activeWorkspace)

const author = computed(() => props.message?.author)
const messageAgeDisplay = computed(() => {
    return `${props.message?.posted === false ? '(sending...)' : dayjs().to(new Date().toString())}`
})

// Convert the message markdown to HTML
const response = (JSON.parse(props.message?.body || '') as KbResponse)
const kbEntries = new Array<KbEntry>()

if (response.answers[0].score === 0) {
    // This occurs when no response is available to the question.
    kbEntries.push({
        prompts: [],
        title: '',
        kbId: 0,
        relevance: 0,
        source: '',
        bodyHtml: "I couldn't find a good response to your question; try asking it differently."
    })
}
else {
    response.answers.forEach(answer => {
        const htmlRender = marked.parse(answer.answer)

        kbEntries.push({
            prompts: answer.context.prompts,
            title: answer.questions[0],
            kbId: answer.id,
            relevance: Math.ceil(answer.score / 20) + 1, // This corresponds to mdi-hexagon-slice-#
            source: answer.source,
            bodyHtml: htmlRender
        })
    })
}

const contents = ref(kbEntries)
const tab = ref('0')

/**
 * Called when a reply message is saved as a note.
 */
async function copyMessageToSave() {
    const index = Number.parseInt(tab.value) // Get the tab value which is the index we want.

    // TODO: Maybe a better way to do this?  Basically, we keep only the selected entry
    const entryToSave = kbEntries.slice(index, index + 1)

    $store.data.setPendingEntry(entryToSave[0]);

    await $router.push({ name: 'NoteEditorDialog' });
}

/**
 * Ask a followup question based on a prompt for this answer
 */
async function askFollowup(prompt: KbFollowupPrompt) {
    const message: Message = {
        id: md5(`${Date.now().toString()}${identity.value?.id}`),
        workspaceId: workspace.value?.id || '',
        createdUtc: dayjs.utc().format(),
        author: identity.value?.asRef || new GenericRef(),
        body: prompt.displayText,
        targetId: prompt.qnaId.toString(),
        posted: false,
        messageType: MessageType.USER_QUESTION_DIRECT
    }

    await $store.messaging.sendQuestion(message);
}

/**
 * Gets the source file name and truncates the file name to 24 characters
 * if necessary and adds an ellipsis
 */
function getSource(entry: KbEntry): string | undefined {
    const source = workspace.value?.sources?.find(s => {
        return s.blobStorageFileName?.endsWith(entry.source)
    })

    if (source && source.displayName && source.displayName?.length > 24) {
        return `${source.displayName.substring(0, 24)}&hellip;`
    }

    return source?.displayName || '[unnamed]'
}

/**
 * User has provided a value to be saved as an alternate question or text related to
 * a given response.  This action is used to train the knowledgebase by sending
 * feedback to the KB.  See:
 *
 * https://docs.microsoft.com/en-us/rest/api/cognitiveservices/qnamakerv5.0-preview.1/knowledgebase/train
 *
 * @param {string} alternateQuestion The question text entered by the user as an alternate
 */
function saveSuggestion(/*alternateQuestion: string*/) {
    // TODO: Add model training feedback loop
}

/**
 * Function to validate the suggested alternate question that the response answers.
 *
 * @param {string} alternateQuestion The question text entered by the user as an alternate
 */
function validateSuggestion(/*alternateQuestion: string*/) {
    // TODO: Implement validation for saving of the suggestion; check length
    return true
}
</script>

<style>
    .message-body-html pre {
        white-space: pre-wrap;       /* Since CSS 2.1 */
        white-space: -moz-pre-wrap;  /* Mozilla, since 1999 */
        white-space: -pre-wrap;      /* Opera 4-6 */
        white-space: -o-pre-wrap;    /* Opera 7 */
        word-wrap: break-word;       /* Internet Explorer 5.5+ */
    }
</style>