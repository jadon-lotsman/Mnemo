import { ref, onMounted, onUnmounted, computed } from 'vue'

const activeInput = ref<HTMLInputElement | HTMLTextAreaElement | null>(null)
const hasActiveInput = computed(() => activeInput.value != null)

export function useActiveInput() {
  const handleFocus = (event: FocusEvent) => {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement
    if (target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA')) {
      activeInput.value = target
    }
  }

  const handleBlur = () => {
    activeInput.value = null
  }

  onMounted(() => {
    document.addEventListener('focusin', handleFocus)
    document.addEventListener('focusout', handleBlur)
  })

  onUnmounted(() => {
    document.removeEventListener('focusin', handleFocus)
    document.removeEventListener('focusout', handleBlur)
  })

  return {
    activeInput,
    hasActiveInput,
  }
}
