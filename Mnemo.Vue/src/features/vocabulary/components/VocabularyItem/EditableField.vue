<script lang="ts" setup>
import { watch } from 'vue'

const props = defineProps<{
  modelValue: string
  prevValue?: string
  placeholder: string
  isEditorMode: boolean
}>()

const emits = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

watch(
  () => props.prevValue,
  () => {
    emits('update:modelValue', '')
  },
)
</script>

<template>
  <div class="editable-wrapper">
    <input
      v-if="isEditorMode"
      type="text"
      :value="modelValue"
      :placeholder="prevValue || placeholder"
      @input="emits('update:modelValue', ($event.target as HTMLInputElement).value)"
      @click.stop
    />
    <div v-else>{{ modelValue || prevValue || '' }}</div>
  </div>
</template>

<style lang="scss" scoped>
.editable-wrapper {
  word-break: break-all;
  text-wrap-mode: wrap;
  text-wrap-style: stable;

  input {
    color: $black-font;
    background-color: transparent;

    height: 20px;
    width: 100%;

    font-size: 16px;
    line-height: 1;
  }
}
</style>
