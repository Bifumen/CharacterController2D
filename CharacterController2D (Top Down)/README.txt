This character controller is meant to be used with kinematic rigidbodies. When the two gameobjects collide they won't push each other like dynamic rigidbodies it will just stop their movement in that direction.

Just modify CharacterController2D.velocity instead of Rigidbody2D.velocity or use the CharacterController2D.MovePosition() method to manually move position.