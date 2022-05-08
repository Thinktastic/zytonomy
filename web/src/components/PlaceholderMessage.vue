<template>
    <QChatMessage
        :name="author?.name"
        :stamp="messageAgeDisplay"
        :text="[message?.body]"
        sent
        text-color="black"
        bg-color="grey-3"
        :size="$q.screen.lt.md ? '9' : '8'"
    >
        <template v-slot:avatar>
            <QAvatar
                size="48px"
                class="q-mx-sm"
                square
                rounded
                color="primary"
                text-color="white"
                icon="mdi-robot-outline"
            />
        </template>

        <template v-slot:default>
            <div class="row">
                <!--// This is a question from a user; render with an icon. //-->
                <div class="col-grow">
                    <QSpinnerBars color="primary" size="1.5em" />
                    {{ message?.body }}
                </div>
            </div>
        </template>
    </QChatMessage>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import dayjs from 'dayjs'
import { Message } from '../models/viewModels'

const props = defineProps({
    message: Message
});

const author = computed(() => props.message?.author)
const messageAgeDisplay = computed(() => dayjs().to(new Date().toString()))
</script>
