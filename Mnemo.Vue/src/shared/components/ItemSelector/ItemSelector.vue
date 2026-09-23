<script setup lang="ts" generic="T">
import { ref } from 'vue'
import type { SelectorItem } from './SelectorItem'
import { useContextMenu } from '@/shared/composables/useContextMenu'
import type { MenuConfig } from '@/features/contextMenu/types/MenuConfig'

const props = defineProps<{
  selected: SelectorItem<T> | null
  icon: string
  config: MenuConfig
}>()

const { openContextByElement } = useContextMenu()

const selector = ref<HTMLElement | null>(null)

function openList() {
  openContextByElement(selector.value, props.config)
}
</script>

<template>
  <div ref="selector" class="selector-container" @click="openList">
    <span class="icon">{{ icon }}</span>
    <div class="selected-item">
      <span class="label">{{ selected?.label || 'None' }}</span>
      <span class="chevron">chevron_forward</span>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.selector-container {
  display: flex;
  align-items: center;

  gap: 5px;

  cursor: pointer;

  margin-bottom: 6px;
  padding-left: 3px;

  color: $text-secondary;

  .icon {
    @include iconize(16px);
  }

  .selected-item {
    display: flex;
    position: relative;
    align-items: center;

    margin-top: 1px;

    min-width: 0;

    font-size: 16px;

    .label {
      @include ellipsis;

      padding-right: 22px;

      min-width: 0;
    }

    .chevron {
      @include iconize(19px);

      position: absolute;

      top: 1px;
      right: 0px;

      transform: rotate(90deg);
      margin-left: -3px;

      color: $text-muted;
    }
  }
}
</style>
