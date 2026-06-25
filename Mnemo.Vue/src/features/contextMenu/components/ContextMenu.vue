<script setup lang="ts">
import type { ContextMenuItem } from '../types/ContextMenuItem'

const props = defineProps<{
  isOpen: boolean
  x: number
  y: number
  items: ContextMenuItem[]
  descriptions: string[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

function handleItemClick(item: ContextMenuItem) {
  if (item.disabled) return
  item.action()
  emit('close')
}
</script>

<template>
  <Teleport to="body">
    <Transition name="context-fade">
      <div
        v-if="isOpen"
        class="context-menu"
        :style="{ top: y + 'px', left: 22 + x + 'px' }"
        @click.stop
      >
        <div v-for="contextItem in props.items" :key="contextItem.label">
          <div
            class="item"
            :class="{ disable: contextItem.disabled }"
            @mousedown.prevent
            @click="handleItemClick(contextItem)"
          >
            <span class="label">{{ contextItem.label }}</span>
            <span class="icon">{{ contextItem.icon }}</span>
          </div>
        </div>
        <div class="descriptions">
          <span v-for="descr in descriptions" :key="descr">{{ descr }}.</span>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style lang="scss" scoped>
.context-menu {
  position: fixed;
  user-select: none;

  display: flex;
  flex-direction: column;

  gap: 5px;

  background-color: #ffffff;
  box-shadow: 5px 5px 0px $shadow;
  border-radius: 0px 10px 10px 10px;

  min-width: 180px;

  padding: 8px;

  z-index: 9999;

  &::after {
    content: '';

    position: absolute;

    width: 8px;
    height: 8px;

    top: 0px;
    left: -22px;

    border-radius: 50%;

    background-color: inherit;
  }

  &::before {
    content: '';

    position: absolute;

    width: 18px;
    height: 18px;

    top: 0px;
    left: -9px;

    border-radius: 50%;

    background-color: inherit;
  }

  .item {
    cursor: pointer;

    display: flex;
    align-items: center;
    justify-content: space-between;

    gap: 12px;

    border-radius: 8px;

    padding: 4px 12px 5px 10px;

    font-style: italic;

    .icon {
      @include iconize-text;

      color: $shadow;
      opacity: 85%;

      font-size: 21px;
      line-height: 0.8;
    }
  }

  .item.disable {
    cursor: default;

    color: $shadow;

    .icon {
      opacity: 65%;
    }
  }

  .descriptions {
    display: flex;
    flex-direction: column;

    gap: 3px;

    color: $shadow;

    padding: 4px 8px 4px 10px;

    font-size: 14px;
  }
}

.context-fade-enter-active,
.context-fade-leave-active {
  transition: all 0.18s ease;
}
.context-fade-enter-from,
.context-fade-leave-to {
  opacity: 0%;
  transform: scale(0.97);
}
</style>
