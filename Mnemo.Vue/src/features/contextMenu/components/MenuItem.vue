<script setup lang="ts">
defineProps<{
  icon?: string
  label: string
  disabled?: boolean
}>()

defineEmits<{ (e: 'click'): void }>()
</script>

<template>
  <div
    class="menu-item"
    :class="{ disabled }"
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
    @include iconize;

    margin-right: 8px;
    margin-left: 4px;

    color: $shadow-color;
    font-size: 21px;
    line-height: 0.8;
  }

  .label {
    @include ellipsis;
    min-width: 0;
  }

  &:hover {
    background-color: $surface-secondary;
  }

  &.disabled {
    cursor: default;

    color: $text-muted;

    .icon {
      opacity: 65%;
    }
  }
}
</style>
