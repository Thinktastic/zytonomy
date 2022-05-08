<template>
    <QPage padding class="row justify-center">
        <div class="col col-md-2 text-center q-pt-lg">
            <QImg
                src="/icons/favicon-128x128.png"
                :ratio="1"
                style="max-width: 128px"
                class="self-center"/>
            <QBtn
                label="LOGIN"
                @click="login"
                flat
                size="lg"
                color="dark"
                icon="mdi-key-outline"
                class="q-mt-lg full-width">
            </QBtn>
        </div>
    </QPage>
</template>

<script setup lang="ts">
import { inject, computed } from 'vue'
import { MsalWrapper } from '../boot/msal';
import { useStore } from '../stores'

const $store = useStore()
const msal = inject<MsalWrapper>('msal')

const identity = computed<string>(() => $store.app.identity?.id || 'User is not logged in')

function login() {
    msal?.loginRedirect()
}
</script>
