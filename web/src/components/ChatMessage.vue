<template>
    <QChatMessage
        :name="author.name"
        :stamp="messageAgeDisplay"
        :text="[message.body]"
        :sent="!userIsSender"
        :text-color="messageTextColor"
        :bg-color="messageBgColor"
        :size="$q.screen.lt.md ? '9' : '8'">
        <template v-slot:avatar>
            <QAvatar
                size="48px"
                class="q-mx-sm" square rounded
                icon="mdi-account-circle-outline">
            </QAvatar>
        </template>

        <template v-slot:default>
            <div class="row">
                <!--// This is a question from a user; render with an icon. //-->
                <div class="col-grow">{{ message?.body }}</div>
                <div class="col-shrink"
                    v-if="isQuestion">
                    <QIcon
                        color="white"
                        size="sm"
                        name="mdi-message-question"/>
                </div>
            </div>
        </template>
    </QChatMessage>
</template>

<script setup lang="ts">
import { useStore } from '../stores'
import { computed } from 'vue'
import dayjs from 'dayjs'
import { Identity, Message, MessageType } from '../models/viewModels'

const props = defineProps<{
    message: Message
}>();

const $store = useStore();

const identity = computed<Identity>(() => $store.app.identity || new Identity())
const author = computed(() => props.message?.author)
const messageAgeDisplay = computed(() => {
    return `${props.message?.posted === false ? '(sending...)' : dayjs().to(new Date().toString())}`
})

const userIsSender = computed(() => identity.value.id === props.message?.author.id)
const messageBgColor = computed(() => userIsSender.value ? 'purple-6' : 'grey-3' )
const messageTextColor = computed(() => userIsSender.value ? 'white' : 'black')

const isQuestion = computed(() => props.message?.messageType !== MessageType.USER_CHAT)
</script>
