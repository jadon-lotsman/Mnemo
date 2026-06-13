<script lang="ts" setup>
import { computed, ref, watch } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue: string
    prevValue: string
    options: string[]
    isEditorMode: boolean
    initialOpen?: boolean
  }>(),
  {
    initialOpen: false,
  },
)

const emits = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const isOpen = ref<boolean>(props.initialOpen)

const displayValue = computed(() => {
  const raw = props.modelValue || props.prevValue
  return raw ? raw.slice(0, 3) + '.' : ''
})

const displayOptions = computed(() =>
  props.options.map((opt) => ({
    value: opt,
    label: opt.slice(0, 5) + '.',
  })),
)

function toggleOpen() {
  isOpen.value = !isOpen.value
}

function selectOption(option: string) {
  emits('update:modelValue', option)
  isOpen.value = false
}

watch(
  () => props.initialOpen,
  (newVal) => {
    isOpen.value = newVal
  },
)

watch(
  () => props.prevValue || props.isEditorMode,
  () => {
    isOpen.value = false
  },
)
</script>

<template>
  <div class="editable-wrapper">
    <div v-if="!isEditorMode" class="part-of-speech">
      <span v-if="modelValue !== '' || prevValue !== 'unknown'"> ({{ displayValue }}) </span>
    </div>
    <div v-else class="select" :class="{ 'select--open': isOpen }">
      <div class="select-input" @click.stop="toggleOpen()">
        <span class="collapse-chevron" :class="{ 'collapse-chevron--open': isOpen }"
          >chevron_forward</span
        >
        {{ displayValue }}
      </div>
      <ul v-if="isOpen" class="options">
        <li
          v-for="option in displayOptions"
          :key="option.value"
          @click.stop="selectOption(option.value)"
        >
          {{ option.label }}
        </li>
      </ul>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.editable-wrapper {
  position: relative;

  text-align: right;

  .collapse-chevron {
    @include iconize-text;

    color: $gray-font;

    transition: transform 0.1s ease;

    position: absolute;
    top: 0px;
    left: 0px;

    &--open {
      transform: rotate(90deg);
    }
  }

  .select {
    position: relative;

    user-select: none;

    background-color: $plane-gray;

    border-radius: 12px;

    width: 65px;

    .select-input {
      padding: 3px 10px;
    }

    .options {
      position: absolute;

      border-radius: 0px 0px 12px 12px;

      width: 65px;

      background-color: $plane-gray;

      padding: 2px 10px;
    }

    &--open {
      border-radius: 12px 12px 0px 0px;
    }
  }
}
</style>
