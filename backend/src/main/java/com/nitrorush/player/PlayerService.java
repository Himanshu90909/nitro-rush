package com.nitrorush.player;

import com.nitrorush.auth.User;
import com.nitrorush.common.ApiException;
import com.nitrorush.common.ErrorCode;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.UUID;

@Service
public class PlayerService {

    private final PlayerProfileRepository profileRepository;

    public PlayerService(PlayerProfileRepository profileRepository) {
        this.profileRepository = profileRepository;
    }

    @Transactional
    public PlayerProfile createProfile(User user, String username) {
        PlayerProfile existing = profileRepository.findByUserId(user.getId()).orElse(null);
        if (existing != null) {
            return existing;
        }
        if (profileRepository.findByUsernameIgnoreCase(username).isPresent()) {
            username = username + "-" + user.getId().toString().substring(0, 4);
        }
        return profileRepository.save(new PlayerProfile(user, username));
    }

    /** Level curve: xpForLevel(n) = 100 * n^1.5 (same curve as the Unity client). */
    public static int levelForXp(long xp) {
        return (int) Math.max(1, Math.floor(Math.pow(xp / 100.0, 1.0 / 1.5)) + 1);
    }

    @Transactional
    public PlayerDtos.ProfileResponse updateProfile(UUID userId, PlayerDtos.UpdateProfileRequest request) {
        PlayerProfile profile = getProfileEntity(userId);
        if (request.username() != null && !request.username().isBlank()) {
            profileRepository.findByUsernameIgnoreCase(request.username())
                    .filter(other -> !other.getUser().getId().equals(userId))
                    .ifPresent(other -> {
                        throw ApiException.of(ErrorCode.VALIDATION_FAILED, "Username already taken");
                    });
            profile.setUsername(request.username().trim());
        }
        if (request.headline() != null) {
            profile.setHeadline(request.headline().trim());
        }
        profile.setLevel(levelForXp(profile.getXp()));
        return toDto(profileRepository.save(profile));
    }

    @Transactional(readOnly = true)
    public PlayerDtos.ProfileResponse getProfile(UUID userId) {
        return toDto(getProfileEntity(userId));
    }

    @Transactional(readOnly = true)
    public PlayerProfile getProfileEntity(UUID userId) {
        return profileRepository.findByUserId(userId)
                .orElseThrow(() -> ApiException.of(ErrorCode.RESOURCE_NOT_FOUND, "Player profile not found"));
    }

    public PlayerDtos.ProfileResponse toDto(PlayerProfile profile) {
        return new PlayerDtos.ProfileResponse(profile.getId(), profile.getUser().getId(),
                profile.getUsername(), profile.getLevel(), profile.getXp(),
                profile.getCredits(), profile.getPremiumTokens(), profile.getHeadline());
    }
}
