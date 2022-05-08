<template>
    <QChip
        square
        clickable
        icon-selected="mdi-check"
        v-model:selected="selected">
        <QAvatar
            icon="mdi-account-circle-outline">
        </QAvatar>
        {{ user.name }}
    </QChip>
</template>

<script setup lang="ts">
import { watch, ref } from 'vue'

import { GenericRef } from '../models/viewModels';

const props = defineProps<{
    user: GenericRef,
    checked: boolean
}>();

const emit = defineEmits(['user-selected', 'user-deselected']);

const selected = ref<boolean>(props.checked)

watch(selected, (value) => {
    if (value) {
        emit('user-selected');
    }
    else {
        emit('user-deselected');
    }
});
</script>
