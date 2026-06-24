<script setup lang="ts">
import Repetition from '@/features/repetition/components/RepetitionComponent.vue'
import CollapsibleSection from '@/shared/components/CollapsibleSection.vue'

window.scrollTo(0, 0)
</script>

<template>
  <CollapsibleSection title="Tasks">
    <Repetition />
  </CollapsibleSection>
</template>

<style lang="scss" scoped></style>
