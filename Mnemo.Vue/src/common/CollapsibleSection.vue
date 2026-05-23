<script setup lang="ts">
import { ref, watch } from 'vue'

const props = withDefaults(
  defineProps<{
    title: string
    initialOpen?: boolean
  }>(),
  {
    initialOpen: true,
  },
)

const emit = defineEmits(['toggle'])

const isOpen = ref(props.initialOpen)

function toggle() {
  isOpen.value = !isOpen.value
  emit('toggle', isOpen.value)
}

watch(
  () => props.initialOpen,
  (newVal) => {
    isOpen.value = newVal
  },
)
</script>

<template>
  <div class="collapsible">
    <div class="header" @click="toggle()">
      <span class="collapse-chevron" :class="{ 'collapse-chevron--open': isOpen }"
        >chevron_forward</span
      >
      <div class="left-text-slot">
        <slot name="title">{{ title }}</slot>
      </div>
      <div class="right-text-slot">
        <slot name="subtext" />
      </div>
    </div>
    <div class="content" :class="{ 'content--open': isOpen }">
      <slot />
    </div>
  </div>
</template>

<style lang="scss" scoped>
.collapsible {
  position: relative;

  width: 100%;

  .collapse-chevron {
    color: $gray-font;

    transition: transform 0.1s ease;

    position: absolute;
    top: -3px;
    left: -5px;

    font-size: 20px;

    font-family: $iconizeFont;

    &--open {
      transform: rotate(90deg);
    }
  }

  .header {
    display: flex;
    justify-content: space-between;

    cursor: pointer;
    user-select: none;

    color: $gray-font;
    margin: 20px 0px 10px 0px;
    margin-left: 18px;

    font-size: 16px;

    .left-text-slot {
      font-size: inherit;
      font-weight: 400;
    }

    .rught-text-slot {
      font-size: inherit;
      font-weight: 300;
    }
  }

  .content {
    display: none;

    &--open {
      display: block;
    }
  }
}
</style>
