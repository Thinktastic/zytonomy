<template>
    <QDialog
        position="right"
        full-height
        persistent
        square
        :maximized="$q.screen.lt.md"
        v-model="visible">
        <QCard
            class="column full-height full-width no-wrap"
            :style="$q.screen.lt.md ? `min-width: ${$q.screen.width}px` : 'min-width: 800px'">
            <QToolbar class="bg-white text-dark">
                <QAvatar>
                    <QIcon name="mdi-message-outline" />
                </QAvatar>
                <QToolbarTitle>{{ workspace?.name }}</QToolbarTitle>
                <QBtn flat round icon="mdi-close" @click="$router.go(-1)"></QBtn>
            </QToolbar>

            <transition
                appear
                enter-active-class="animated fadeInDown"
                leave-active-class="animated fadeOutUp">
                <QCardSection
                    v-if="messages?.length === 0"
                    horizontal
                    class="col-shrink bg-grey-2 row text-dark q-pa-none q-mx-md">
                    <QCardSection>
                        <QIcon name="mdi-blur" size="md"/>
                    </QCardSection>
                    <QSeparator vertical inset color="dark"/>
                    <QCardSection class="text-body2">
                        Hi <strong>{{ identity?.firstName }}</strong>, get started by asking a question below.  I'll try to help you by
                        answering your questions.
                    </QCardSection>
                </QCardSection>
            </transition>

            <QCardSection class="col q-pt-none">
                <!--// Display the chat interface //-->
                <div class='col q-px-md' style='height: 100%'>
                    <QScrollArea style='height: 100%;' :thumb-style="{width: '5px', borderRadius: '2px', right: '1px'}" ref="chat">
                        <!--
                        <chat-message
                            v-for="(message, index) in messages"
                            :key="`${index}_${message.entity.id}`"
                            :message="message" :kbResponse="false" />
                        -->
                        <!--//
                            https://stackoverflow.com/a/50419362/116051
                        //-->

                        <Component
                            v-for="(message, index) in messages"
                            :is="resolveComponent(message)"
                            :key="`${message.id}`"
                            :message="message"/>

                    </QScrollArea>
                </div>
            </QCardSection>

            <QCardActions class="bg-white">
                <QInput
                    v-model='typedMessage'
                    outlined
                    bottom-slots
                    :label="$q.screen.lt.md ? 'Type your message here' : 'Type your question or chat here (CTRL+ENTER to send chat)'"
                    type='textarea'
                    autogrow
                    class="full-width"
                    @keyup.enter="checkSubmitMessage(true)"
                    @keyup.ctrl.enter="checkSubmitMessage(false)"
                    clearable
                    autofocus>
                    <template v-slot:after>
                        <QBtn round flat icon='mdi-send' @click="checkSubmitMessage(false)">
                            <QTooltip anchor="top start" self="center middle">Send chat message</QTooltip>
                        </QBtn>
                        <QBtn round flat icon='mdi-message-question-outline' @click="checkSubmitMessage(true)">
                            <QTooltip anchor="top start" self="center middle">Send question</QTooltip>
                        </QBtn>
                    </template>
                </QInput>
            </QCardActions>
        </QCard>
    </QDialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import PlaceholderMessage from './PlaceholderMessage.vue'
import AnswerMessage from './AnswerMessage.vue'
import ChatMessage from './ChatMessage.vue'
import { useRoute } from 'vue-router'
import { useStore } from '../stores'
import { GenericRef, Message, MessageType } from '../models/viewModels'
import md5 from 'md5'
import dayjs from 'dayjs'
import { plainToClass } from 'class-transformer'
import { QScrollArea } from 'quasar'

const chat = ref<QScrollArea>() // Ref for the chat scroll area.
const $route = useRoute()
const $store = useStore()

const visible = computed(() => $route.name == 'ChatDialog')
const identity = computed(() => $store.app.identity)
const workspace = computed(() => $store.data.activeWorkspace)
const messages = computed(() => $store.messaging.activeMessageList?.messages)
const messageCount = computed(() => $store.messaging.activeMessageList?.messages.length || 0)
const typedMessage = ref('')
const routeParams = computed(() => $route.params);

watch(routeParams, (newVal) => {
    const workspaceId = $store.data.activeWorkspace?.id
    let messageList = $store.messaging.messageLists.find(l => l.workspaceId === workspaceId)

    if(!messageList) {
        $store.messaging.addAndSetActiveMessageList(workspaceId || '')
    }
    else {
        $store.messaging.setActiveMessageList(messageList)
    }
})

watch(messageCount, () => {
    console.log(`  MESSAGE COUNT IS: ${messageCount.value}`)
    window.setTimeout( () => chat.value?.setScrollPercentage('vertical', 1.0, 300), 500)
})

watch(visible, (value) => {
    if (value) {
        window.setTimeout( () => chat.value?.setScrollPercentage('vertical', 1.0, 300), 500)
    }
})

async function checkSubmitMessage(isQuestion: boolean) {
    const text = typedMessage.value

    if (text.length === 0) {
        return // do nothing.
    }

    // Display in chat first and clear the value.
    typedMessage.value = ''

    if(isQuestion) {
        await sendQuestion(text)
    }
    else {
        await sendMessage(text)
    }
}

async function sendQuestion(text: string) {
    console.log(`  CHAT CURRENT WORKSPACE: ${workspace.value?.id}`)
    console.log(`  STORE ACTIV WORKSPACE: ${$store.data.activeWorkspace?.id}`)

    const message: Message = plainToClass(Message, {
        id: md5(`${Date.now().toString()}${identity.value?.id}`),
        workspaceId: workspace.value?.id || '',
        createdUtc: dayjs.utc().format(),
        author: identity.value?.asRef || new GenericRef(),
        body: text,
        posted: false,
        messageType: MessageType.USER_QUESTION
    })

    await $store.messaging.sendQuestion(message);
}

async function sendMessage(text: string) {
    console.log(`  CHAT CURRENT WORKSPACE: ${workspace.value?.id}`)
    console.log(`  STORE ACTIV WORKSPACE: ${$store.data.activeWorkspace?.id}`)

    const message: Message = plainToClass(Message, {
        id: md5(`${Date.now().toString()}${identity.value?.id}`),
        workspaceId: workspace.value?.id || '',
        createdUtc: dayjs.utc().format(),
        author: identity.value?.asRef || new GenericRef(),
        body: text,
        posted: false,
        messageType: MessageType.USER_CHAT
    })

    await $store.messaging.sendMessage(message);
}

function resolveComponent(message: Message) {
    switch(message.messageType) {
        case MessageType.USER_CHAT:
        case MessageType.USER_QUESTION:
        case MessageType.USER_QUESTION_DIRECT:
            return ChatMessage
        case MessageType.BOT_PLACEHOLDER:
            return PlaceholderMessage
        case MessageType.BOT_RESPONSE:
            return AnswerMessage
    }
}
</script>

<style type="text/css">
.video-box {
    max-height: 227px;
}
</style>
