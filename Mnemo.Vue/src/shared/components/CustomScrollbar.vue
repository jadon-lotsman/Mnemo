<script setup lang="ts">
import { computed, ref } from 'vue'
import { useElementSize, useScroll } from '@vueuse/core'

const props = withDefaults(
  defineProps<{
    thickness?: number
    gap?: number
    minThumbHeight?: number
  }>(),
  {
    thickness: 3,
    gap: 6,
    minThumbHeight: 20,
  },
)

const containerRef = ref<HTMLElement | null>(null)
const contentRef = ref<HTMLElement | null>(null)

const { y: scrollTop } = useScroll(containerRef)
const { height: viewportHeight } = useElementSize(containerRef)
const { height: contentHeight } = useElementSize(contentRef)

const hasScroll = computed(() => contentHeight.value > viewportHeight.value + 1)

const thumbHeight = computed(() => {
  if (!hasScroll.value || contentHeight.value === 0) return 0
  const ratio = viewportHeight.value / contentHeight.value
  return Math.max(viewportHeight.value * ratio, props.minThumbHeight)
})

const thumbTop = computed(() => {
  if (!hasScroll.value) return 0
  const maxScroll = contentHeight.value - viewportHeight.value
  const maxThumbTop = viewportHeight.value - thumbHeight.value
  if (maxScroll <= 0 || maxThumbTop <= 0) return 0
  return (scrollTop.value / maxScroll) * maxThumbTop
})

const isDragging = ref(false)
let dragStartY = 0
let dragStartScrollTop = 0

function onThumbPointerDown(e: PointerEvent) {
  isDragging.value = true
  dragStartY = e.clientY
  dragStartScrollTop = containerRef.value?.scrollTop ?? 0
  ;(e.currentTarget as HTMLElement).setPointerCapture(e.pointerId)
}

function onThumbPointerMove(e: PointerEvent) {
  if (!isDragging.value || !containerRef.value) return
  const maxScroll = contentHeight.value - viewportHeight.value
  const maxThumbTop = viewportHeight.value - thumbHeight.value
  if (maxThumbTop <= 0) return
  const deltaY = e.clientY - dragStartY
  containerRef.value.scrollTop = dragStartScrollTop + (deltaY / maxThumbTop) * maxScroll
}

function onThumbPointerUp(e: PointerEvent) {
  isDragging.value = false
  ;(e.currentTarget as HTMLElement).releasePointerCapture(e.pointerId)
}
</script>

<template>
  <div class="custom-scrollbar">
    <div
      ref="containerRef"
      class="scroll-container"
      :style="{ paddingRight: `${thickness + gap}px` }"
    >
      <div ref="contentRef">
        <slot />
      </div>
    </div>

    <Transition name="thumb-fade">
      <div
        v-show="hasScroll"
        class="thumb"
        :class="{ 'is-dragging': isDragging }"
        :style="{
          minWidth: `${thickness}px`,
          width: `${thickness}px`,
          height: `${thumbHeight}px`,
          top: `${thumbTop}px`,
        }"
        @pointerdown="onThumbPointerDown"
        @pointermove="onThumbPointerMove"
        @pointerup="onThumbPointerUp"
        @pointercancel="onThumbPointerUp"
      />
    </Transition>
  </div>
</template>

<style scoped lang="scss">
.custom-scrollbar {
  position: relative;

  width: 100%;
  height: 100%;

  overflow: hidden;
}

.scroll-container {
  width: 100%;
  height: 100%;

  overflow-x: hidden;
  overflow-y: auto;

  scrollbar-width: none;

  &::-webkit-scrollbar {
    display: none;
  }
}

.thumb {
  position: absolute;

  right: 0px;

  opacity: 0.8;

  cursor: pointer;

  border-radius: 999px;
  background-color: $icon-color;
  user-select: none;

  &:hover,
  &.is-dragging {
    opacity: 1;
  }
}

.thumb-fade-enter-active,
.thumb-fade-leave-active {
  transition: opacity 0.1s ease;
}
.thumb-fade-enter-from,
.thumb-fade-leave-to {
  opacity: 0;
}
</style>
