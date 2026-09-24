<script setup lang="ts">
import { nextTick, ref, watch, type ComponentPublicInstance } from 'vue'
import type { VocabularyRange } from '../types/VocabularySector'
import { useVocabularyStore } from '../stores/VocabularyStore'
import type { VocabularyHeader } from '../types/VocabularyHeader'

const props = defineProps<{
  header: VocabularyHeader | null
  disabled?: boolean
}>()

const vocabularyStore = useVocabularyStore()
const indicatorReady = ref<boolean>(false)

const isDescending = ref<boolean>(false)
const letterRange = defineModel<VocabularyRange | null>('letterRange', { default: null })

function toggleDescending() {
  isDescending.value = !isDescending.value
}

const tabRefs = ref<Record<string, HTMLElement | null>>({})
function setTabRef(el: Element | ComponentPublicInstance | null, label: string) {
  tabRefs.value[label] = el as HTMLElement | null
}

const indicatorX = ref(0)
const indicatorW = ref(0)

function moveIndicator(range: VocabularyRange | null) {
  const el = tabRefs.value[range?.label ?? '']
  if (!el) return

  indicatorX.value = el.offsetLeft
  indicatorW.value = el.offsetWidth

  if (!indicatorReady.value) {
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        indicatorReady.value = true
      })
    })
  }
}

watch(
  () => props.header,
  async () => {
    isDescending.value = false
    await vocabularyStore.fetchRanges(props.header?.guid ?? null, isDescending.value)
    letterRange.value = vocabularyStore.ranges[0] ?? null
  },
)
watch(
  () => isDescending.value,
  async () => {
    const oldIndex = vocabularyStore.ranges.findIndex(
      (t) => t.startWord === letterRange.value?.startWord,
    )
    const newIndex = vocabularyStore.ranges.length - 1 - oldIndex
    await vocabularyStore.fetchRanges(props.header?.guid ?? null, isDescending.value)
    letterRange.value = vocabularyStore.ranges[newIndex] ?? vocabularyStore.ranges[0] ?? null
  },
)

watch(
  () => letterRange.value,
  async () => {
    await nextTick()

    const current = vocabularyStore.ranges.find((t) => t.startWord === letterRange.value?.startWord)
    if (current) moveIndicator(current)
  },
  { immediate: true },
)
</script>

<template>
  <div class="nav-container">
    <div class="ranges-container">
      <div
        class="indicator"
        :class="{ ready: indicatorReady }"
        :style="{ transform: `translateX(${indicatorX}px)`, width: `${indicatorW}px` }"
      ></div>

      <label
        class="tab"
        :class="{ disabled: disabled || vocabularyStore.loadingPlaceholder.isLoading }"
        v-for="tab in vocabularyStore.ranges"
        :key="tab.label"
        :ref="(el) => setTabRef(el, tab.label)"
      >
        <input
          type="radio"
          name="range"
          v-model="letterRange"
          :value="tab"
          :disabled="disabled || vocabularyStore.loadingPlaceholder.isLoading"
        />
        <span>{{ tab.label }}</span>
      </label>
    </div>
    <button
      class="descending-button"
      :class="{ flipped: isDescending }"
      :disabled="disabled"
      @click="toggleDescending"
    >
      sort
    </button>
  </div>
</template>

<style lang="scss" scoped>
.nav-container {
  display: flex;
  justify-content: space-between;

  margin: 15px 6px 12px 6px;

  border-bottom: 3px solid $surface-secondary;
  border-radius: 2px;

  .ranges-container {
    display: flex;
    position: relative;
    justify-content: start;
    align-items: end;

    gap: 7px;

    margin-bottom: -1px;

    .indicator {
      position: absolute;

      opacity: 0;
      z-index: 0;

      will-change: transform, width;

      border-radius: 8px 8px 0px 0px;
      background-color: $surface-secondary;

      min-width: 25px;
      height: 100%;

      pointer-events: none;

      &.ready {
        opacity: 1;
        transition:
          transform 0.25s ease,
          width 0.15s linear;
      }
    }

    .tab {
      position: relative;

      z-index: 1;

      cursor: pointer;

      background-color: transparent;

      padding: 5px 7px 3px 7px;

      color: $text-muted;

      font-size: 16px;

      input {
        display: none;
      }

      span {
        user-select: none;
      }

      &:has(input:checked) {
        color: $text-secondary;

        span {
          display: inline-block;
          transform: translateY(-2px);
        }
      }

      &.disabled {
        opacity: 0.5;
        color: $text-secondary;
      }
    }
  }

  .descending-button {
    @include iconize(18px);

    margin-top: 5px;

    background-color: transparent;
  }

  .descending-button.flipped {
    transform: scaleY(-1);
  }
}
</style>
