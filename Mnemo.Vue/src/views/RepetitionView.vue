<script setup lang="ts">
import Repetition from '@/features/repetition/components/RepetitionComponent.vue'
import Finish from '@/features/repetition/components/FinishComponent.vue'
import CollapsibleSection from '@/shared/components/CollapsibleSection.vue'
</script>

<template>
  <CollapsibleSection title="Tasks">
    <Repetition />
  </CollapsibleSection>
  <Finish />
</template>

<style lang="scss" scoped></style>
