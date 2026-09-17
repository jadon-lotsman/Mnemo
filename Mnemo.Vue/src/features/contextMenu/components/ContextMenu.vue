<script setup lang="ts">
import { useContextMenu } from '@/shared/composables/useContextMenu'
import type { ContextMenuOption } from '../types/ContextMenuOption'
import { computed } from 'vue'

const {
  isVisible,
  menuX,
  menuY,
  hasTriangle,
  isLeftAligned,
  isTopAligned,
  menuOptions,
  menuDetails,
  closeMenu,
} = useContextMenu()

const triangleClass = computed(() => {
  if (!hasTriangle.value) return 'no-triangle'

  const vertical = isTopAligned.value ? 'bottom' : 'top'
  const horizontal = isLeftAligned.value ? 'right' : 'left'
  return `triangle-${vertical}-${horizontal}`
})

function invokeOption(item: ContextMenuOption) {
  if (item.disabled) return

  item.action()
  closeMenu()
}
</script>

<template>
  <Teleport to="body">
    <Transition name="context-fade">
      <div
        v-if="isVisible"
        class="context-menu"
        :class="triangleClass"
        :style="{ top: menuY + 'px', left: menuX + 'px' }"
        @click.stop
      >
        <div class="triangle"></div>
        <header>
          <div v-for="contextItem in menuOptions" :key="contextItem.label">
            <div
              class="item"
              :class="{ disabled: contextItem.disabled }"
              @mousedown.prevent
              @click="invokeOption(contextItem)"
            >
              <span class="icon">{{ contextItem.icon }}</span>
              <span class="label">{{ contextItem.label }}</span>
            </div>
          </div>
        </header>
        <footer v-if="menuDetails.length">
          <div class="descriptions">
            <span v-for="info in menuDetails" :key="info">{{ info }}.</span>
          </div>
        </footer>
      </div>
    </Transition>
  </Teleport>
</template>

<style lang="scss" scoped>
.context-menu {
  display: flex;
  position: absolute;
  flex-direction: column;

  z-index: 9999;

  backdrop-filter: blur(2px);
  filter: drop-shadow(0px 0px 8px #bbbbbb4d) drop-shadow(5px 5px 0px $shadow-color);

  will-change: transform, opacity;

  border-radius: 12px;
  background-color: $elevated-bg;

  padding: 7px 6px 10px 6px;
  min-width: 220px;

  user-select: none;

  header {
    display: flex;
    flex-direction: column;
    gap: 2px;

    .item {
      display: flex;
      align-items: center;
      cursor: pointer;

      border-radius: 8px;

      padding: 4px;

      .icon {
        @include iconize;

        margin-right: 12px;
        margin-left: 8px;

        color: $shadow-color;

        font-size: 21px;
        line-height: 0.8;
      }

      &:hover {
        background-color: $surface-secondary;
      }
    }

    .item.disabled {
      cursor: default;

      color: $shadow-color;

      .icon {
        opacity: 65%;
      }
    }
  }

  footer {
    .descriptions {
      display: flex;
      flex-direction: column;
      gap: 3px;

      margin-top: 6px;
      margin-left: 10px;

      color: $text-secondary;

      font-size: 15px;
    }
  }
}

.triangle {
  display: block;
  position: absolute;

  background-color: transparent;

  width: 0;
  height: 0;
}

@mixin triangle-corner($v, $h) {
  .triangle {
    border: 8px solid transparent;
    border-#{$v}: 8px solid $elevated-bg;
    @if $h == left {
      border-right: 8px solid $elevated-bg;
    } @else {
      border-left: 8px solid $elevated-bg;
    }
    #{$v}: 0px;
    #{$h}: -12px;
  }
}

.triangle-top-left {
  border-top-left-radius: 0 !important;
  @include triangle-corner(top, left);
}
.triangle-top-right {
  border-top-right-radius: 0 !important;
  @include triangle-corner(top, right);
}
.triangle-bottom-left {
  border-bottom-left-radius: 0 !important;
  @include triangle-corner(bottom, left);
}
.triangle-bottom-right {
  border-bottom-right-radius: 0 !important;
  @include triangle-corner(bottom, right);
}

.context-fade-enter-active {
  transition: all 0.18s ease;
}
.context-fade-leave-active {
  transition: all 0.28s ease;
}
.context-fade-enter-from,
.context-fade-leave-to {
  transform: scale(0.97);
  opacity: 0%;
}
</style>
