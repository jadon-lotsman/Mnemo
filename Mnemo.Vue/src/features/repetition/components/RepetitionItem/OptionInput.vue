<script setup lang="ts">
withDefaults(
  defineProps<{
    modelValue: string
    placeholder: string
    value: string
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
  <label
    class="option"
    :class="{ 'has-placeholder': modelValue === '' && placeholder === value, disabled: disabled }"
  >
    <input
      type="radio"
      name="option"
      :value="value"
      :checked="modelValue === value"
      :disabled="disabled"
      @change="$emit('update:modelValue', value)"
    />
    <div>{{ value }}</div>
  </label>
</template>

<style lang="scss" scoped>
.disabled {
  cursor: default !important;
}

.option {
  cursor: pointer;

  input {
    display: none;
  }

  div {
    display: flex;
    position: relative;

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
      @include iconize;

      position: absolute;
      top: -3px;
      left: -3px;

      opacity: 0%;

      content: 'check';
      color: $text-secondary;

      font-size: 26px;
    }
  }

  input:checked + div::after {
    opacity: 100%;
  }
}

.has-placeholder {
  input:not(:checked) + div::after {
    opacity: 65%;
  }
}
</style>
