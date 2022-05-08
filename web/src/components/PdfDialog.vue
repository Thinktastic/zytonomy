<template>
    <QDialog
        @hide="onDialogHide"
        :full-height="!$q.screen.lt.md"
        :full-width="!$q.screen.lt.md"
        :maximized="$q.screen.lt.md"
        square
        v-model="visible"
        transition-show="slide-up"
        transition-hide="slide-down"
        >
        <div class="column" style="background: white">
            <div class="col col-auto">
                <QToolbar class="bg-white text-dark">
                    <QToolbarTitle>PDF</QToolbarTitle>
                    <QBtn flat round icon="mdi-close" @click="$router.back()"></QBtn>
                </QToolbar>
            </div>
            <div class="col">
                <PdfViewer
                    v-if="showPdf"
                    v-model="showPdf"
                    :src="url"
                    type="pdfjs"/>
            </div>
        </div>
    </QDialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useStore } from '../stores'
import { onBeforeRouteUpdate, useRoute } from 'vue-router'
import PdfViewer from './PdfViewer.vue'

const $route = useRoute()
const $store = useStore()
const showPdf = ref(false)

const visible = computed(() => $route.name === 'PdfDialog')
const url = computed(() => $store.data.securePdfUrl || '')

// Need to process and retrieve the PDF URL
watch(visible, async (newValue) => {
    if(!newValue) {
        return;
    }

    if(!$route.params.sourceIndex)
    {
        return;
    }

    const index = Number.parseInt($route.params.sourceIndex as string);

    // This call will build a URL with a timed SAS token to read the document directly from Azure Storage.
    await $store.data.getSecureDocumentUrl(index)
})

function onDialogHide() {
    // Need to clear the URL.
}

watch(url, (newValue) => {
    console.log(`PDF URL: ${newValue}`);

    if(newValue && newValue !== '') {
        showPdf.value = true
    }
})
</script>
