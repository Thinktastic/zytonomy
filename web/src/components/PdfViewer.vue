<template>
    <div class="q-pdfviewer">
        <iframe
            class="q-pdfviewer__iframe"
            :src="`pdfjs/web/viewer.html?file=${encodeURIComponent(props.src)}`"
            width="100%"
            height="100%">

        </iframe>
    </div>
</template>

<script setup lang="ts">
const props = defineProps<{
    src: string
}>();
</script>

<style>
.q-pdfviewer {
    overflow: "hidden";
    height: 100%;
    width: 100%;
}

.q-pdfviewer__iframe {
    border: 0;
    height: 100%;
    left: 0;
    top: 0;
    width: 100%;
}
</style>