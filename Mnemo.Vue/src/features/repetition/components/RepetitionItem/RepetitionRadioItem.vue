<script setup lang="ts">
defineProps<{
  modelValue: string
  value: string
}>()

defineEmits<{
  (e: 'update:modelValue', query: string): void
}>()
</script>

<template>
  <label class="answer-radio">
    <input
      type="radio"
      name="option"
      :value="value"
      :checked="modelValue === value"
      @change="$emit('update:modelValue', value)"
    />
    <div>{{ value }}</div>
  </label>
</template>

<style lang="scss" scoped>
.answer-radio {
  input {
    display: none;
  }

  div {
    position: relative;

    display: flex;

    &::before {
      content: '';
      display: block;

      background-color: $plane-gray;

      border-radius: 4px;
      margin-right: 10px;

      width: 20px;
      height: 20px;
    }

    &::after {
      @include iconize-text;

      content: 'check';
      color: $gray-font;

      opacity: 0%;

      position: absolute;
      top: -3px;
      left: -3px;

      font-size: 26px;
    }
  }

  input:checked + div::after {
    opacity: 100%;
  }
}
</style>
