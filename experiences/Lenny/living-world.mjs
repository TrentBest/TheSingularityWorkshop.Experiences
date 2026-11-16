// Renderer-neutral interaction state for Little Lenny.
// Animation and hit-testing belong to the presentation host; this module owns
// the authoritative, idempotent cleanup state transitions.

export function createCleanupState() {
  return {
    phase: "idle",
    pendingItemId: null,
    heldItemId: null,
    activeOperationId: null,
    depositedItemIds: []
  };
}

function unchanged(state) { return state; }

export function reduceCleanupState(state, event) {
  if (!state || !event || typeof event.type !== "string") return state;

  switch (event.type) {
    case "SELECT_LITTER": {
      if (state.phase !== "idle" || !event.itemId || !event.operationId) return unchanged(state);
      if (state.depositedItemIds.includes(event.itemId)) return unchanged(state);
      return { ...state, phase: "approaching-item", pendingItemId: event.itemId, activeOperationId: event.operationId };
    }
    case "PICKUP_COMPLETED": {
      if (state.phase !== "approaching-item" || event.operationId !== state.activeOperationId) return unchanged(state);
      return { ...state, phase: "carrying", heldItemId: state.pendingItemId, pendingItemId: null, activeOperationId: null };
    }
    case "SELECT_RECEPTACLE": {
      if (state.phase !== "carrying" || !state.heldItemId || !event.operationId) return unchanged(state);
      return { ...state, phase: "approaching-receptacle", activeOperationId: event.operationId };
    }
    case "DEPOSIT_COMPLETED": {
      if (state.phase !== "approaching-receptacle" || event.operationId !== state.activeOperationId || !state.heldItemId) return unchanged(state);
      const depositedItemIds = state.depositedItemIds.includes(state.heldItemId)
        ? state.depositedItemIds
        : [...state.depositedItemIds, state.heldItemId];
      return { ...state, phase: "idle", heldItemId: null, pendingItemId: null, activeOperationId: null, depositedItemIds };
    }
    case "INTERRUPT": {
      // Before pickup completion, the item remains available. Once picked up,
      // interruption never drops or duplicates it: Lenny resumes carrying it.
      if (state.phase === "approaching-item") {
        return { ...state, phase: "idle", pendingItemId: null, activeOperationId: null };
      }
      if (state.phase === "approaching-receptacle") {
        return { ...state, phase: "carrying", activeOperationId: null };
      }
      return unchanged(state);
    }
    case "NAVIGATE_AWAY": {
      // Story navigation is independent of cleanup. Preserve a carried item,
      // but cancel any not-yet-completed pickup attempt.
      if (state.heldItemId) return { ...state, phase: "carrying", pendingItemId: null, activeOperationId: null };
      return { ...state, phase: "idle", pendingItemId: null, activeOperationId: null };
    }
    default:
      return unchanged(state);
  }
}

export function getDepositedCount(state) {
  return state?.depositedItemIds?.length ?? 0;
}
