<script setup lang="ts">
defineProps<{
  modelValue: string
  value: string
  title: string
  description: string
}>()

defineEmits<{
  (e: 'update:modelValue', query: string): void
}>()
</script>

<template>
  <label class="mode-radio">
    <input
      type="radio"
      name="mode"
      :value="value"
      :checked="modelValue === value"
      @change="$emit('update:modelValue', value)"
    />
    <div class="title">{{ title }}</div>
    <div class="description">{{ description }}</div>
  </label>
</template>

<style lang="scss" scoped>
.mode-radio {
  @include lift();

  cursor: pointer;

  box-shadow: 5px 5px 0px $shadow-color;

  border-radius: 12px;

  background-color: $surface-primary;
  padding: 10px;

  user-select: none;

  input {
    display: none;
  }

  .title {
    display: flex;

    position: relative;
    padding-bottom: 20px;
    color: $text-primary;

    font-size: 16px;

    &::before {
      display: block;
      margin-right: 10px;

      border-radius: 4px;

      background-color: $surface-secondary;

      width: 20px;
      height: 20px;
      content: '';
    }

    &::after {
      @include iconize(19px);

      position: absolute;
      top: 0px;
      left: 0px;

      opacity: 0%;

      content: 'check';

      color: $text-secondary;
    }
  }

  .description {
    max-width: 90%;
    color: $text-secondary;
    font-weight: 400;

    font-size: 14px;
  }

  input:checked + .title::after {
    opacity: 100%;
  }
}
</style>
