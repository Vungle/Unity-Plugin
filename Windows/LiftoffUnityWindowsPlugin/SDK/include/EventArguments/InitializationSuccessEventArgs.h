#pragma once

#include <vector>

#include "PlacementConfigFromServer.h"

class InitializationSuccessEventArgs {
public:
    std::vector<PlacementConfigFromServer> Placements;

    InitializationSuccessEventArgs(const std::vector<PlacementConfigFromServer>& placements = {})
        : Placements(placements) {}
};
