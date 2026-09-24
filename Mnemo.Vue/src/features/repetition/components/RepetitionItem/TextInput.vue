<script setup lang="ts">
withDefaults(
  defineProps<{
    modelValue: string
    placeholder: string
    disabled?: boolean
  }>(),
  {
    disabled: false,
  },
)

defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()
</script>

<template>
  <input
    type="text"
    class="text-input"
    autocorrect="off"
    autocomplete="off"
    spellcheck="false"
    :value="modelValue"
    :placeholder="placeholder"
    :disabled="disabled"
    @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
  />
</template>

<style lang="scss" scoped>
.text-input {
  border-radius: 8px;
  background-color: $surface-secondary;
  padding: 7px 10px;

  width: 100%;
  color: $text-primary;

  font-size: 15px;
}
</style>
