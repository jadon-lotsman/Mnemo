import { ref, onUnmounted } from 'vue'

export function useLoadingPlaceholer(delay: number = 300) {
  const isLoading = ref(false)
  const showSkeleton = ref(false)
  let timer: ReturnType<typeof setTimeout> | null = null

  const startLoading = () => {
    isLoading.value = true
    showSkeleton.value = false
    if (timer) clearTimeout(timer)
    timer = setTimeout(() => {
      if (isLoading.value) {
        showSkeleton.value = true
      }
    }, delay)
  }

  const stopLoading = () => {
    isLoading.value = false
    showSkeleton.value = false
    if (timer) {
      clearTimeout(timer)
      timer = null
    }
  }

  onUnmounted(() => {
    if (timer) clearTimeout(timer)
  })

  return {
    isLoading,
    showSkeleton,
    startLoading,
    stopLoading,
  }
}
