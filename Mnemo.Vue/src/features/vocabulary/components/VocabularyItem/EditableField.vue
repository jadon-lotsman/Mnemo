<script lang="ts" setup>
import { watch } from 'vue'

const props = defineProps<{
  modelValue: string
  prevValue: string
  placeholder: string
  isEditorMode: boolean
}>()

const emits = defineEmits<{
  (e: 'update:modelValue', query: string): void
}>()

watch(
  () => props.prevValue,
  () => {
    emits('update:modelValue', '')
  },
)
</script>

<template>
  <div>
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
input {
  color: $black-font;
  background-color: transparent;

  height: 20px;
  width: 80%;

  font-size: 16px;
}
</style>
