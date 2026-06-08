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
    :value="modelValue"
    :placeholder="placeholder"
    :disabled="disabled"
    @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
  />
</template>

<style lang="scss" scoped>
.text-input {
  background-color: $plane-gray;
  color: $black-font;

  border-radius: 8px;
  padding: 7px 10px;

  width: 100%;

  font-size: 15px;
}
</style>
