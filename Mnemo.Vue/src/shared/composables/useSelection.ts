import { ref, onMounted, onUnmounted } from 'vue'

export function useSelection() {
  const hasSelection = ref(false)
  const selectedText = ref('')

  const updateSelection = () => {
    const selection = window.getSelection()
    const text = selection?.toString()?.trim() || ''
    selectedText.value = text
    hasSelection.value = text.length > 0
  }

  const handleSelectionChange = () => {
    updateSelection()
  }

  const handleMouseUp = () => {
    setTimeout(updateSelection, 0)
  }

  onMounted(() => {
    document.addEventListener('selectionchange', handleSelectionChange)
    document.addEventListener('mouseup', handleMouseUp)
    updateSelection()
  })

  onUnmounted(() => {
    document.removeEventListener('selectionchange', handleSelectionChange)
    document.removeEventListener('mouseup', handleMouseUp)
  })

  return {
    hasSelection,
    selectedText,
  }
}
