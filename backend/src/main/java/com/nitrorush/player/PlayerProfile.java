package com.nitrorush.player;

import com.nitrorush.auth.User;
import jakarta.persistence.*;
import org.hibernate.annotations.CreationTimestamp;
import org.hibernate.annotations.UpdateTimestamp;

import java.time.Instant;
import java.util.UUID;

/** Persistent player progression: level, XP and the two currencies. */
@Entity
@Table(name = "player_profiles", uniqueConstraints = @UniqueConstraint(name = "uk_profile_user", columnNames = "user_id"))
public class PlayerProfile {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @OneToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "user_id", nullable = false)
    private User user;

    @Column(nullable = false, length = 32)
    private String username;

    private int level = 1;
    private long xp = 0;

    /** Standard earned currency. */
    @Column(nullable = false)
    private long credits = 5000;

    /** Premium currency. */
    @Column(nullable = false)
    private int premiumTokens = 100;

    @Column(length = 120)
    private String headline;

    @CreationTimestamp
    private Instant createdAt;

    @UpdateTimestamp
    private Instant updatedAt;

    public PlayerProfile() {
    }

    public PlayerProfile(User user, String username) {
        this.user = user;
        this.username = username;
    }

    public UUID getId() { return id; }
    public User getUser() { return user; }
    public String getUsername() { return username; }
    public void setUsername(String username) { this.username = username; }
    public int getLevel() { return level; }
    public void setLevel(int level) { this.level = level; }
    public long getXp() { return xp; }
    public void setXp(long xp) { this.xp = xp; }
    public long getCredits() { return credits; }
    public void setCredits(long credits) { this.credits = credits; }
    public int getPremiumTokens() { return premiumTokens; }
    public void setPremiumTokens(int premiumTokens) { this.premiumTokens = premiumTokens; }
    public String getHeadline() { return headline; }
    public void setHeadline(String headline) { this.headline = headline; }
    public Instant getCreatedAt() { return createdAt; }
    public Instant getUpdatedAt() { return updatedAt; }
}
