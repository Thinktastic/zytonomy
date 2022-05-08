<template>
    <QCard flat>
        <QItem :style="{ 'border-top' : `8px solid ${getColor}` }">
            <QItemSection>
                <QItemLabel>
                    <QChip square
                        clickable
                        @click="applyUserFilter(note.author)">
                        <QAvatar
                            icon="mdi-account-circle-outline">
                            <!--
                            <img :src="author.avatar"/>
                            -->
                        </QAvatar>
                        {{ note.author.name }}
                    </QChip>
                </QItemLabel>
            </QItemSection>

            <QItemSection side>
                <QBtn
                    :icon="note.icon"
                    size="md"
                    flat
                    round
                    :color="note.color"
                    dense
                    @click="applyIconFilter(note.icon)"/>
            </QItemSection>

            <!--
            <QItemSection side>
                <QBadge>
                    V2
                </QBadge>
            </QItemSection>
            -->

            <QItemSection side v-if="viewing">
                <QBtn flat round dense icon="mdi-close" @click="handleClose"/>
            </QItemSection>
        </QItem>

        <QCardSection
            @click="viewNote"
            :class="{ 'cursor-pointer' : !viewing }">
            <QItemLabel lines="1"
                :class="viewing ? 'text-h5' : 'text-subtitle2'"
                :style="[$q.screen.gt.sm ? { } : { 'width' : `${$q.screen.width - 60}px` } ]">
                {{ note.name }}
            </QItemLabel>

            <QSeparator v-if="viewing" class="q-my-sm"/>

            <!--// Body text display style based on whether showing summary or viewing full content //-->
            <div
                v-if="!viewing && note.firstVideoUrl != null"
                style="height: 50px"
                class="q-mt-xs text-center">
                <QIcon
                    name="mdi-youtube"
                    size="xl"
                    color="grey-6"/>
            </div>
            <QItemLabel
                v-else-if="!viewing"
                lines="3"
                style="height: 50px">
                {{ bodyText }}
            </QItemLabel>
            <QScrollArea
                 v-else
                 :style="{ 'height' : $q.screen.lt.md ? '400px' : '440px' }"
                 :thumb-style="{width: '5px', borderRadius: '2px', right: '1px'}">
                <div v-html="bodyText"></div>
            </QScrollArea>
        </QCardSection>

        <QCardSection class="q-py-none">
            <QItemLabel lines="1" style="min-height: 29px;">
                <QChip
                    dense
                    outline
                    square
                    size="md"
                    color="secondary"
                    clickable
                    v-for="(tag, index) in note.tags"
                    :key="`tag-${index}`"
                    @click="applyTagFilter(tag)">
                    #{{ tag }}
                </QChip>
            </QItemLabel>
        </QCardSection>

        <QCardActions>
            <div class='row' style='width: 100%'>
                <div class='col-grow'>
                    <!--
                    <QBtn flat icon='mdi-bookmark-outline' color='grey-6' dense></QBtn>
                    -->
                    <QBtn flat icon='mdi-comment-outline' color='grey-6' dense @click="viewNoteComments">
                        <QBadge
                            floating
                            v-if="note.comments && note.comments.length > 0"
                            :label="note.comments.length"
                            color="pink" />
                    </QBtn>
                    <QBtn flat icon='mdi-cog-outline' color='grey-6' title="Note actions">
                        <QMenu
                            auto-close>
                            <QList>
                                <QItem clickable @click="editNote">
                                    <QItemSection avatar>
                                        <QIcon name="mdi-pencil-outline" color="grey-7"/>
                                    </QItemSection>
                                    <QItemSection>Edit</QItemSection>
                                </QItem>
                                <QItem clickable>
                                    <QItemSection avatar>
                                        <QIcon name="mdi-archive-outline" color="grey-7"/>
                                    </QItemSection>
                                    <QItemSection>Archive</QItemSection>
                                </QItem>
                                <QItem clickable @click="deleteNote">
                                    <QItemSection avatar>
                                        <QIcon name="mdi-delete-outline" color="red-8"/>
                                    </QItemSection>
                                    <QItemSection class="text-red-8">Delete</QItemSection>
                                </QItem>
                            </QList>
                        </QMenu>
                    </QBtn>
                </div>
                <div class='col-shrink q-pt-sm text-caption'>
                    {{ messageAgeDisplay }}
                </div>
            </div>
        </QCardActions>
    </QCard>
</template>

<script setup lang="ts">
import { colors } from 'quasar'
import { useStore } from '../stores'
import { GenericRef, Note } from '../models/viewModels';
import { computed } from 'vue';
import { stripHtml } from 'string-strip-html'
import dayjs from 'dayjs'

const props = defineProps<{
    note: Note,
    viewing: boolean
}>();

const emit = defineEmits(['view-note', 'view-comments', 'close-note']);

const $store = useStore()

// Body text of the message with the HTML stripped out.
const bodyText = computed(() => {
    // If the message starts with <iframe then it is a video
    // TODO: Come up with a better way to do this
    let contents = props.note?.body

    // eslint-disable-next-line
    return props.viewing
        ? contents
        : stripHtml(contents || '').result
})

// Converts a color label to a standard HTML color hex code
const getColor = computed(() => {
    return colors.getPaletteColor(props.note?.color || 'white')
})

// The displayed age of the mesage.
const messageAgeDisplay = computed(() => {
    const age = Date.parse(props.note?.createdUtc || new Date().toString())

    return dayjs(age).fromNow()
})

// Handles the click event on the message note card to load the note for display.
function viewNote() {
    // TODO: Maybe can remove this?
    //await rootStore.dispatch('messaging/viewNoteAsNote', props.message)

    emit('view-note', props.note) // Do not scroll to comments
}

// Handles the click event on the message note card to load the note for display.
function viewNoteComments() {
    // TODO: Maybe can remove this?
    //await rootStore.dispatch('messaging/viewNoteAsNote', props.message)

    emit('view-comments', props.note) // Scroll to comments
}

// Handles deltion of this note.
function deleteNote() {
    $store.data.deleteNote(props.note);
}

// Handles the click event on the close icon when the message is in viewing mode.
function handleClose() {
    emit('close-note')
}

// Applies a tag filter to the search when the user clicks on a tag.
function applyTagFilter(tag: string) {
    $store.data.addFilter({
        label: tag,
        value: tag,
        type: 'tag'
    });

    if (props.viewing) {
        emit('close-note')
    }
}

// Applies an icon filter when the user clicks on the icon.
function applyIconFilter(icon: string) {
    $store.data.addFilter({
        label: icon,
        value: icon,
        type: 'icon'
    });

    if (props.viewing) {
        emit('close-note')
    }
}

// Applies a user filter when the user clicks on the author.
function applyUserFilter(author: GenericRef) {
    $store.data.addFilter({
        label: author.name,
        value: author.id,
        avatar: 'mdi-account-circle-outline', // TODO: User avatar here.
        type: 'user'
    });

    if (props.viewing) {
        emit('close-note')
    }
}

// Triggers an edit of the note by changing the route.
function editNote() {
    //await rootStore.dispatch('messaging/copyMessageToSave', prop.note)

    //await root.$router.push({ name: 'NoteEditorDialog' })
}
</script>
