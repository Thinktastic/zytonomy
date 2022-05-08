<template>
    <QDialog
        position="right"
        full-height
        :maximized="$q.screen.lt.md"
        persistent
        square
        @show="scrollToComments"
        v-model="visible">
        <QCard
            v-if="note"
            class="column full-height full-width no-wrap"
            :style="$q.screen.lt.md ? `min-width: ${$q.screen.width}px` : 'min-width: 800px'">

            <!--// The note body //-->
            <NoteCard
                :note="note"
                :viewing="true"
                @close-note="closeDialog" />

            <!--// Chat input //-->
            <QCardActions class="bg-white">
                <QInput
                    v-model='textMessage'
                    outlined
                    bottom-slots
                    label='Type your message here'
                    type='textarea'
                    autogrow
                    class="full-width"
                    @keyup.enter='submitMessage'
                    clearable
                    ref="commentInput">
                    <template v-slot:after>
                        <QBtn round flat icon='mdi-send' @click="submitMessage"/>
                    </template>
                </QInput>
            </QCardActions>

            <!--// Display for the comments //-->
            <div v-if="note.comments && note.comments.length > 0">
                <QCardSection
                    v-for="(comment, index) in note.comments"
                    :key="index"
                    :horizontal="$q.screen.gt.md"
                    class="q-pa-none">
                    <QCardSection style="min-width: 200px" class="q-pb-none">
                        <QChip square>
                            <QAvatar icon="mdi-account-circle-outline">
                            </QAvatar>
                            {{ comment.author?.name }}
                        </QChip>
                        <QItemLabel class="q-my-xs text-caption">
                            {{ convertToDisplayDate(comment.createdUtc) }}
                        </QItemLabel>
                    </QCardSection>
                    <QCardSection>
                        {{ comment.body }}
                    </QCardSection>
                </QCardSection>
            </div>
        </QCard>
    </QDialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'

import dayjs from 'dayjs'

import NoteCard from '../components/NoteCard.vue'

import { useStore } from '../stores'
import { useRouter, useRoute } from 'vue-router'
import { Note, Comment } from '../models/viewModels'

const $store = useStore()
const $router = useRouter()
const $route = useRoute()

const identity = computed(() => $store.app.identity)
const visible = computed(() => $route.name === 'NoteDialog')
const note = ref<Note>()

watch(visible, (newVal) => {
    if(!newVal) {
        return
    }

    note.value = $store.data.activeWorkspaceNotes.find(n => n.id === $route.params.noteId)
})

const commentInput = ref()
const textMessage = ref('')

function scrollToComments () {
    const viewComments = $route.params.comments === 'comments'

    if (!commentInput.value || !viewComments) {
        return
    }

    commentInput.value.$el.scrollIntoView(true, { behavior: 'smooth' })
}

function closeDialog() {
    $router.back()
}

function convertToDisplayDate(created: string) {
    return dayjs().to(created)
}

async function submitMessage() {
    // When sending a related message, the root message is set as the conversation ID.
    const comment = new Comment()
    comment.author = identity.value?.asRef || null
    comment.body = textMessage.value
    comment.parentId = note.value?.id || '' // TODO: Add ability to reply to existing comment.

    // Send the comment.
    await $store.data.addComment({ comment: comment, noteId: note.value?.id || '' });

    textMessage.value = ''
}
</script>
