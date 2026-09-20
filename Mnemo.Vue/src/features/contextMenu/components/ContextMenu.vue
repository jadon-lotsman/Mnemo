<script setup lang="ts">
import { useContextMenu } from '@/shared/composables/useContextMenu'
import { computed } from 'vue'
import ModuleRender from './ModuleRender.vue'

const {
  isVisible,
  isPositioned,
  menuX,
  menuY,
  hasTriangle,
  isLeftAligned,
  isTopAligned,
  menuModules,
  menuRef,
} = useContextMenu()

const triangleClass = computed(() => {
  if (!hasTriangle.value) return 'no-triangle'

  const vertical = isTopAligned.value ? 'bottom' : 'top'
  const horizontal = isLeftAligned.value ? 'right' : 'left'
  return `triangle-${vertical}-${horizontal}`
})
</script>

<template>
  <Teleport to="body">
    <Transition name="context-fade">
      <div
        v-if="isVisible"
        ref="menuRef"
        class="context-menu"
        :class="[triangleClass, { 'is-measuring': !isPositioned }]"
        :style="{ top: menuY + 'px', left: menuX + 'px' }"
        @click.stop
      >
        <div class="triangle"></div>
        <div class="module-container">
          <ModuleRender v-for="(mod, i) in menuModules?.modules" :key="i" :module="mod" />
        </div>
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

  will-change: transform, opacity, filter;

  border-radius: 12px;
  background-color: $elevated-bg;

  padding: 7px 6px 10px 6px;

  min-width: 220px;
  max-width: 330px;

  user-select: none;

  .module-container {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }
}

&.is-measuring {
  visibility: hidden;
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
    border: 7px solid transparent;
    border-#{$v}: 7px solid $elevated-bg;
    @if $h == left {
      border-right: 7px solid $elevated-bg;
    } @else {
      border-left: 7px solid $elevated-bg;
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
  transition:
    transform 0.18s ease,
    opacity 0.18s ease;
}
.context-fade-leave-active {
  transition:
    transform 0.18s ease,
    opacity 0.18s ease;
}
.context-fade-enter-from,
.context-fade-leave-to {
  transform: scale(0.97);
  opacity: 0%;
}
</style>
