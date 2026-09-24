<script setup lang="ts">
defineProps<{
  icon?: string
  label: string
  selected?: boolean
  disabled?: boolean
}>()

defineEmits<{ (e: 'click'): void }>()
</script>

<template>
  <div
    class="menu-item"
    :class="[{ selected }, { disabled }]"
    @mousedown.prevent
    @click="!disabled && $emit('click')"
  >
    <span v-if="icon" class="icon">{{ icon }}</span>
    <span class="label">{{ label }}</span>
  </div>
</template>

<style lang="scss" scoped>
.menu-item {
  display: flex;
  align-items: center;
  cursor: pointer;

  border-radius: 8px;

  padding: 4px 8px 4px 4px;

  .icon {
    @include iconize(16px);

    margin-right: 12px;
    margin-left: 8px;
  }

  .label {
    @include ellipsis;
    min-width: 0;
  }

  &.selected {
    &::after {
      content: 'check';

      @include iconize(16px);
      margin-left: auto;

      padding-left: 6px;

      color: $text-muted;
    }
  }

  &.disabled {
    cursor: default;
    color: $text-muted;

    .icon {
      color: $shadow-color;
    }
  }

  &:not(.disabled) {
    &:hover {
      background-color: $surface-secondary;
    }
  }
}
</style>
